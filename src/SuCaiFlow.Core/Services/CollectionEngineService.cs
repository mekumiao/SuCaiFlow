using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Events;
using SuCaiFlow.Contracts.Repositories;
using SuCaiFlow.Contracts.Services;

namespace SuCaiFlow.Core.Services;

public partial class CollectionEngineService(
    ICollectionTaskRepository collectionTaskRepository,
    ICollectedAssetRepository assetRepository,
    ITaskExecutionService taskExecutionService,
    ICollectionTaskConfigRepository collectionTaskConfigRepository,
    ISiteCollectorManager siteCollectorManager,
    IEventPublisher eventPublisher,
    ILogger<CollectionEngineService> logger,
    HttpClient httpClient) {
    private readonly ICollectionTaskRepository _collectionTaskRepository = collectionTaskRepository;
    private readonly ICollectedAssetRepository _assetRepository = assetRepository;
    private readonly ITaskExecutionService _taskExecutionService = taskExecutionService;
    private readonly ICollectionTaskConfigRepository _collectionTaskConfigRepository = collectionTaskConfigRepository;
    private readonly ISiteCollectorManager _siteCollectorManager = siteCollectorManager;
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly ILogger<CollectionEngineService> _logger = logger;
    private readonly HttpClient _httpClient = httpClient;

    private readonly ConcurrentDictionary<Guid, int> _progressUpdates = new();
    private readonly SemaphoreSlim _dbUpdateSemaphore = new(1, 1); // 用于保护数据库更新的信号量

    // 用于存储任务进度更新的并发字典，避免在多线程中直接访问EF Core
    private readonly ConcurrentDictionary<Guid, int> _pendingProgressUpdates = new();

    // 用于协调页面解析和资源下载的队列
    private readonly ConcurrentQueue<string> _resourceUrlQueue = new();
    private readonly SemaphoreSlim _queueSemaphore = new(10, 10); // 控制资源下载的并发数

    public async Task ProcessCollectionTaskAsync(Guid taskId, CancellationToken cancellationToken) {
        try {
            // 标记任务为正在运行
            await _taskExecutionService.MarkTaskAsRunningAsync(taskId, cancellationToken);

            var task = await _collectionTaskRepository.GetByIdAsync(taskId, cancellationToken);
            if (task == null) {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError("Collection task with ID {TaskId} not found", taskId);
                await _taskExecutionService.MarkTaskAsFailedAsync(taskId, cancellationToken);
                return;
            }

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Starting collection task {TaskId} for URL: {Url}", taskId, task.Url);

            // 发布任务开始事件
            await _eventPublisher.PublishAsync(new CollectionTaskStartedEvent {
                TaskId = taskId,
                TaskName = task.Name,
                Url = task.Url
            }, cancellationToken);

            // 更新任务状态为进行中
            task.Status = CollectionTaskStatus.InProgress;
            task.StartedAt = DateTime.UtcNow;
            await _collectionTaskRepository.UpdateAsync(task, cancellationToken);
            await _collectionTaskRepository.SaveChangesAsync(cancellationToken);

            // 执行新的并行采集任务
            var assets = await ExecuteParallelCollectionAsync(task, cancellationToken);

            // 更新任务的采集数量
            task.AssetsCollectedCount = assets.Count;

            // 确保最终进度被更新到数据库
            await UpdateTaskProgressInDatabaseAsync(task, cancellationToken);

            // 保存采集到的素材
            foreach (var asset in assets) {
                asset.CollectionTaskId = taskId;
                asset.CreatedAt = DateTime.UtcNow;
                await _assetRepository.AddAsync(asset, cancellationToken);
            }

            await _assetRepository.SaveChangesAsync(cancellationToken);

            // 更新任务状态为完成
            task.Status = CollectionTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.SetNewAssets(assets);
            await _collectionTaskRepository.UpdateAsync(task, cancellationToken);
            await _collectionTaskRepository.SaveChangesAsync(cancellationToken);

            // 标记任务执行完成
            await _taskExecutionService.MarkTaskAsCompletedAsync(taskId, cancellationToken);

            // 发布任务完成事件
            await _eventPublisher.PublishAsync(new CollectionTaskCompletedEvent {
                TaskId = taskId,
                TaskName = task.Name,
                AssetsCollectedCount = assets.Count,
                Status = task.Status
            }, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Completed collection task {TaskId}, collected {AssetCount} assets", taskId, assets.Count);
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error processing collection task {TaskId}", taskId);

            // 更新任务状态为失败
            var task = await _collectionTaskRepository.GetByIdAsync(taskId, cancellationToken);
            if (task != null) {
                task.Status = CollectionTaskStatus.Failed;
                task.ErrorMessage = ex.Message;
                task.CompletedAt = DateTime.UtcNow;
                await _collectionTaskRepository.UpdateAsync(task, cancellationToken);
                await _collectionTaskRepository.SaveChangesAsync(cancellationToken);
            }

            // 发布任务失败事件
            await _eventPublisher.PublishAsync(new CollectionTaskFailedEvent {
                TaskId = taskId,
                TaskName = task?.Name ?? "Unknown",
                ErrorMessage = ex.Message
            }, cancellationToken);

            await _taskExecutionService.MarkTaskAsFailedAsync(taskId, cancellationToken);
        }
    }

    private async Task<List<CollectedAsset>> ExecuteParallelCollectionAsync(CollectionTask task, CancellationToken cancellationToken) {
        var assets = new List<CollectedAsset>();
        var completedParsing = new TaskCompletionSource<bool>();

        try {
            // 启动页面解析任务（带自动翻页）
            var parsingTask = Task.Run(async () => await ParsePagesWithPaginationAsync(task, completedParsing, cancellationToken), cancellationToken);

            // 启动资源下载任务
            var downloadingTask = Task.Run(async () => await ProcessResourceDownloadsAsync(task, assets, completedParsing, cancellationToken), cancellationToken);

            // 等待两个任务完成
            await Task.WhenAll(parsingTask, downloadingTask);
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error executing parallel collection for task {TaskId}", task.Id);
            throw;
        }

        return assets;
    }

    /// <summary>
    /// 解析页面（带自动翻页功能）
    /// </summary>
    private async Task ParsePagesWithPaginationAsync(CollectionTask task, TaskCompletionSource<bool> parsingCompleted, CancellationToken cancellationToken) {
        var parsedUrls = new List<string>();
        var currentPage = 1;
        var baseUrl = task.Url;
        ArgumentNullException.ThrowIfNull(baseUrl, nameof(task.Url));

        // 获取任务配置，如果任务有关联的配置，则使用配置中的参数
        var config = task.ConfigId.HasValue ?
            await _collectionTaskConfigRepository.GetByIdAsync(task.ConfigId.Value, cancellationToken) : null;

        // 确定最大解析条目数 - 优先使用配置，然后是任务参数，最后是默认值
        var maxParseItems = config?.MaxParseItems ??
                           (task.Parameters?.TryGetValue("MaxParseItems", out string? value) == true ?
                               int.Parse(value) : 100);

        // 确定请求延迟 - 优先使用配置
        var requestDelayMs = config?.RequestDelayMs ?? 500;

        // 确定是否启用自动翻页 - 优先使用配置
        var enableAutoPagination = config?.EnableAutoPagination ?? true;

        try {
            // 解析第一页
            var firstPageUrls = await ParsePageAsync(task, baseUrl, config, cancellationToken);
            foreach (var url in firstPageUrls) {
                _resourceUrlQueue.Enqueue(url);
                parsedUrls.Add(url);

                // 创建并保存素材记录到数据库
                var asset = new CollectedAsset {
                    Id = Guid.NewGuid(),
                    Name = Path.GetFileName(new Uri(url).LocalPath) ?? $"asset_{Guid.NewGuid()}",
                    Url = url,
                    CollectionTaskId = task.Id,
                    Status = CollectedAssetStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _assetRepository.AddAsync(asset, cancellationToken);

                // 检查是否已达到最大解析条目数
                if (parsedUrls.Count >= maxParseItems) {
                    break;
                }
            }

            // 保存所有新解析的素材到数据库
            await _assetRepository.SaveChangesAsync(cancellationToken);

            // 如果启用了自动翻页且未达到最大条目数，继续翻页
            while (enableAutoPagination && parsedUrls.Count < maxParseItems) {
                // 构造下一页URL - 优先使用配置中的模式
                string nextPageUrl = config != null && !string.IsNullOrEmpty(config.NextPagePattern)
                    ? ConstructNextPageUrlByPattern(baseUrl, currentPage + 1, config.NextPagePattern)
                    : ConstructNextPageUrl(baseUrl, currentPage + 1);
                if (string.IsNullOrEmpty(nextPageUrl)) {
                    break; // 无法构造下一页URL，停止翻页
                }

                var nextPageUrls = await ParsePageAsync(task, nextPageUrl, config, cancellationToken);
                if (!nextPageUrls.Any()) {
                    break; // 没有更多内容，停止翻页
                }

                foreach (var url in nextPageUrls) {
                    _resourceUrlQueue.Enqueue(url);
                    parsedUrls.Add(url);

                    // 创建并保存素材记录到数据库
                    var asset = new CollectedAsset {
                        Id = Guid.NewGuid(),
                        Name = Path.GetFileName(new Uri(url).LocalPath) ?? $"asset_{Guid.NewGuid()}",
                        Url = url,
                        CollectionTaskId = task.Id,
                        Status = CollectedAssetStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _assetRepository.AddAsync(asset, cancellationToken);

                    // 检查是否已达到最大解析条目数
                    if (parsedUrls.Count >= maxParseItems) {
                        break;
                    }
                }

                // 保存当前页面解析的素材到数据库
                await _assetRepository.SaveChangesAsync(cancellationToken);

                currentPage++;

                // 短暂延迟以避免过于频繁的请求
                await Task.Delay(requestDelayMs, cancellationToken);
            }

            // 更新任务的预期资源数量
            task.TotalAssetsExpected = parsedUrls.Count;
            await _collectionTaskRepository.UpdateAsync(task, cancellationToken);
            await _collectionTaskRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error parsing pages for task {TaskId}", task.Id);
            throw;
        }
        finally {
            // 标记解析完成
            parsingCompleted.SetResult(true);
        }
    }

    /// <summary>
    /// 解析单个页面获取资源URL
    /// </summary>
    private async Task<IEnumerable<string>> ParsePageAsync(CollectionTask task, string pageUrl, CollectionTaskConfig? config, CancellationToken cancellationToken) {
        try {
            // 获取适合当前URL的站点采集器
            var siteCollector = _siteCollectorManager.GetCollectorForUrl(pageUrl);
            if (siteCollector == null) {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("No site collector found for URL: {PageUrl}", pageUrl);
                return [];
            }

            return await siteCollector.ParsePageAsync(task, pageUrl, config, cancellationToken);
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error parsing page {PageUrl} for task {TaskId}", pageUrl, task.Id);
            return [];
        }
    }

    /// <summary>
    /// 根据配置模式构造下一页URL
    /// </summary>
    private string ConstructNextPageUrlByPattern(string baseUrl, int pageNumber, string pattern) {
        // 获取适合当前URL的站点采集器
        var siteCollector = _siteCollectorManager.GetCollectorForUrl(baseUrl);
        if (siteCollector != null) {
            return siteCollector.ConstructNextPageUrlByPattern(baseUrl, pageNumber, pattern);
        }

        // 如果没有找到特定采集器，使用通用方法
        // 这里使用通用站点采集器的实现
        var genericCollector = _siteCollectorManager.GetCollectorByIdentifier("generic");
        if (genericCollector != null) {
            return genericCollector.ConstructNextPageUrlByPattern(baseUrl, pageNumber, pattern);
        }

        // 如果连通用采集器都找不到，使用默认实现
        return ConstructNextPageUrlByPatternDefault(baseUrl, pageNumber, pattern);
    }

    /// <summary>
    /// 构造下一页URL（默认模式）
    /// </summary>
    private string ConstructNextPageUrl(string baseUrl, int pageNumber) {
        // 获取适合当前URL的站点采集器
        var siteCollector = _siteCollectorManager.GetCollectorForUrl(baseUrl);
        if (siteCollector != null) {
            return siteCollector.ConstructNextPageUrl(baseUrl, pageNumber);
        }

        // 如果没有找到特定采集器，使用通用方法
        var genericCollector = _siteCollectorManager.GetCollectorByIdentifier("generic");
        if (genericCollector != null) {
            return genericCollector.ConstructNextPageUrl(baseUrl, pageNumber);
        }

        // 如果连通用采集器都找不到，使用默认实现
        return ConstructNextPageUrlDefault(baseUrl, pageNumber);
    }

    private async Task<List<CollectedAsset>> ExecuteCollectionAsync(CollectionTask task, CancellationToken cancellationToken) {
        var assets = new List<CollectedAsset>();

        try {
            // 获取网页内容
            var htmlContent = await _httpClient.GetStringAsync(task.Url, cancellationToken);

            // 使用正则表达式或HTML解析器根据选择器提取资源链接
            var assetUrls = ExtractAssetUrls(htmlContent, task.Selector);

            // 更新任务的预期资源数量
            task.TotalAssetsExpected = assetUrls.Count;
            await _collectionTaskRepository.UpdateAsync(task, cancellationToken);
            await _collectionTaskRepository.SaveChangesAsync(cancellationToken);

            // 使用并发限制来下载资源
            using var semaphore = new SemaphoreSlim(task.MaxConcurrency);
            var downloadTasks = new List<Task>();

            foreach (var assetUrl in assetUrls.Take(100)) // 限制采集数量，避免过度采集
            {
                downloadTasks.Add(ProcessAssetAsync(semaphore, task, assetUrl, assets, cancellationToken));
            }

            // 启动一个后台任务定期更新进度
            var progressUpdateTask = Task.Run(async () => {
                while (downloadTasks.Any(t => !t.IsCompleted || !t.IsCanceled || !t.IsFaulted)) {
                    await Task.Delay(1000); // 每秒检查一次进度更新
                    await UpdatePendingProgressInDatabaseAsync(task.Id, cancellationToken);
                }
            }, cancellationToken);

            await Task.WhenAll(downloadTasks);

            // 等待进度更新任务完成
            await progressUpdateTask;

            // 确保剩余的进度更新也被写入数据库
            await UpdatePendingProgressInDatabaseAsync(task.Id, cancellationToken);
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error executing collection for task {TaskId}", task.Id);
            throw;
        }

        return assets;
    }

    /// <summary>
    /// 处理资源下载任务
    /// </summary>
    private async Task ProcessResourceDownloadsAsync(CollectionTask task, List<CollectedAsset> assets, TaskCompletionSource<bool> parsingCompleted, CancellationToken cancellationToken) {
        var downloadTasks = new List<Task>();
        var maxConcurrency = task.MaxConcurrency;

        // 启动指定数量的下载器
        for (int i = 0; i < maxConcurrency; i++) {
            downloadTasks.Add(StartDownloaderAsync(task, assets, parsingCompleted, cancellationToken));
        }

        await Task.WhenAll(downloadTasks);
    }

    /// <summary>
    /// 启动单个下载器
    /// </summary>
    private async Task StartDownloaderAsync(CollectionTask task, List<CollectedAsset> assets, TaskCompletionSource<bool> parsingCompleted, CancellationToken cancellationToken) {
        // 获取任务配置
        var config = task.ConfigId.HasValue ?
            await _collectionTaskConfigRepository.GetByIdAsync(task.ConfigId.Value, cancellationToken) : null;
        while (true) {

            // 从队列中获取资源URL
            if (_resourceUrlQueue.TryDequeue(out string? assetUrl)) {
                // 根据URL查找数据库中的素材记录
                var existingAsset = await _assetRepository.GetByUrlAsync(assetUrl, cancellationToken);
                if (existingAsset != null && existingAsset.CollectionTaskId == task.Id) {
                    // 预处理资源URL
                    var absoluteUrl = ResolveUrl(task.Url!, assetUrl);
                    var processedUrl = PreprocessAssetUrl(absoluteUrl, config);

                    // 发布资源下载开始事件
                    await _eventPublisher.PublishAsync(new AssetDownloadStartedEvent {
                        TaskId = task.Id,
                        AssetId = existingAsset.Id,
                        AssetUrl = absoluteUrl
                    }, cancellationToken);

                    // 更新素材状态为采集中
                    existingAsset.Status = CollectedAssetStatus.InProgress;
                    await _assetRepository.UpdateAsync(existingAsset, cancellationToken);
                    await _assetRepository.SaveChangesAsync(cancellationToken);

                    // 使用站点采集器下载资源
                    var downloadedAsset = await DownloadAssetWithSiteCollectorAsync(processedUrl, config, cancellationToken);

                    if (downloadedAsset != null) {
                        // 更新素材状态为已完成
                        existingAsset.Status = CollectedAssetStatus.Completed;
                        existingAsset.Size = downloadedAsset.Size;
                        existingAsset.ContentType = downloadedAsset.ContentType;
                        existingAsset.LocalPath = downloadedAsset.LocalPath;
                        existingAsset.DownloadedAt = DateTime.UtcNow;
                    }
                    else {
                        // 更新素材状态为失败
                        existingAsset.Status = CollectedAssetStatus.Failed;
                        existingAsset.ErrorMessage = "Failed to download asset";
                    }

                    await _assetRepository.UpdateAsync(existingAsset, cancellationToken);
                    await _assetRepository.SaveChangesAsync(cancellationToken);

                    // 发布资源下载完成事件
                    await _eventPublisher.PublishAsync(new AssetDownloadCompletedEvent {
                        TaskId = task.Id,
                        AssetId = existingAsset.Id,
                        AssetUrl = absoluteUrl,
                        Size = existingAsset.Size,
                        ContentType = existingAsset.ContentType,
                        Success = existingAsset.Status == CollectedAssetStatus.Completed,
                        ErrorMessage = existingAsset.ErrorMessage
                    }, cancellationToken);

                    lock (assets) {
                        assets.Add(existingAsset);
                    }

                    // 更新采集计数
                    lock (task) {
                        task.AssetsCollectedCount++;
                    }

                    // 更新待处理的进度计数
                    _pendingProgressUpdates.AddOrUpdate(task.Id, 1, (key, value) => value + 1);
                }
            }
            else {
                // 检查解析是否已完成
                if (parsingCompleted.Task.IsCompleted) {
                    // 如果解析已完成且队列为空，则退出
                    break;
                }

                // 队列为空，短暂等待后重试
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    private async Task ProcessAssetAsync(SemaphoreSlim semaphore, CollectionTask task, string assetUrl, List<CollectedAsset> assets, CancellationToken cancellationToken) {
        await semaphore.WaitAsync(cancellationToken);
        try {
            // 获取任务配置
            var config = task.ConfigId.HasValue ?
                await _collectionTaskConfigRepository.GetByIdAsync(task.ConfigId.Value, cancellationToken) : null;

            var absoluteUrl = ResolveUrl(task.Url!, assetUrl);
            var processedUrl = PreprocessAssetUrl(absoluteUrl, config);
            var asset = await DownloadAssetWithSiteCollectorAsync(processedUrl, config, cancellationToken);

            if (asset != null) {
                lock (assets) {
                    assets.Add(asset);
                }

                // 更新采集计数
                lock (task) {
                    task.AssetsCollectedCount++;
                }

                // 增加进度更新计数
                _progressUpdates.AddOrUpdate(task.Id, 1, (key, value) => value + 1);

                // 更新待处理的进度计数
                _pendingProgressUpdates.AddOrUpdate(task.Id, 1, (key, value) => value + 1);

                // 每当累计更新达到一定数量时，更新数据库
                if (_progressUpdates.TryGetValue(task.Id, out int updateCount) && updateCount >= 5) {
                    _progressUpdates.AddOrUpdate(task.Id, 0, (key, value) => 0); // 重置计数
                }

                // 发布资源下载完成事件
                await _eventPublisher.PublishAsync(new AssetDownloadCompletedEvent {
                    TaskId = task.Id,
                    AssetId = asset.Id,
                    AssetUrl = asset.Url,
                    Size = asset.Size,
                    ContentType = asset.ContentType,
                    Success = true,
                    ErrorMessage = null
                }, cancellationToken);
            }
            else {
                // 发布资源下载失败事件
                await _eventPublisher.PublishAsync(new AssetDownloadCompletedEvent {
                    TaskId = task.Id,
                    AssetId = Guid.Empty, // 未知资源ID
                    AssetUrl = absoluteUrl,
                    Success = false,
                    ErrorMessage = "Failed to download asset"
                }, cancellationToken);
            }
        }
        finally {
            semaphore.Release();
        }
    }

    private async Task UpdatePendingProgressInDatabaseAsync(Guid taskId, CancellationToken cancellationToken) {
        // 检查是否有待处理的进度更新
        if (_pendingProgressUpdates.TryGetValue(taskId, out int progressIncrement) && progressIncrement > 0) {
            // 使用信号量确保只有一个线程可以更新数据库
            await _dbUpdateSemaphore.WaitAsync(cancellationToken);
            try {
                // 从数据库重新加载任务以避免并发问题
                var freshTask = await _collectionTaskRepository.GetByIdAsync(taskId, cancellationToken);
                if (freshTask != null) {
                    freshTask.AssetsCollectedCount += progressIncrement;
                    // 确保计数不会超过预期总数
                    freshTask.AssetsCollectedCount = Math.Min(freshTask.AssetsCollectedCount, freshTask.TotalAssetsExpected);

                    await _collectionTaskRepository.UpdateAsync(freshTask, cancellationToken);
                    await _collectionTaskRepository.SaveChangesAsync(cancellationToken);

                    // 发布进度更新事件
                    var progressEvent = new CollectionTaskProgressEvent {
                        TaskId = freshTask.Id,
                        AssetsCollectedCount = freshTask.AssetsCollectedCount,
                        TotalAssetsExpected = freshTask.TotalAssetsExpected,
                        ProgressPercentage = freshTask.TotalAssetsExpected > 0 ?
                            (double)freshTask.AssetsCollectedCount / freshTask.TotalAssetsExpected * 100 : 0
                    };
                    await _eventPublisher.PublishAsync(progressEvent, cancellationToken);

                    // 从待处理更新中减去已处理的进度
                    _pendingProgressUpdates.AddOrUpdate(taskId, 0, (key, value) => value - progressIncrement);
                }
            }
            catch (Exception ex) {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(ex, "Error updating progress for task {TaskId}", taskId);
            }
            finally {
                _dbUpdateSemaphore.Release();
            }
        }
    }

    private async Task UpdateTaskProgressInDatabaseAsync(CollectionTask task, CancellationToken cancellationToken) {
        // 使用信号量确保只有一个线程可以更新数据库
        await _dbUpdateSemaphore.WaitAsync(cancellationToken);
        try {
            // 从数据库重新加载任务以避免并发问题
            var freshTask = await _collectionTaskRepository.GetByIdAsync(task.Id, cancellationToken);
            if (freshTask != null) {
                freshTask.AssetsCollectedCount = task.AssetsCollectedCount;
                await _collectionTaskRepository.UpdateAsync(freshTask, cancellationToken);
                await _collectionTaskRepository.SaveChangesAsync(cancellationToken);

                // 发布进度更新事件
                var progressEvent = new CollectionTaskProgressEvent {
                    TaskId = freshTask.Id,
                    AssetsCollectedCount = freshTask.AssetsCollectedCount,
                    TotalAssetsExpected = freshTask.TotalAssetsExpected,
                    ProgressPercentage = freshTask.TotalAssetsExpected > 0 ?
                        (double)freshTask.AssetsCollectedCount / freshTask.TotalAssetsExpected * 100 : 0
                };
                await _eventPublisher.PublishAsync(progressEvent, cancellationToken);
            }
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error updating progress for task {TaskId}", task.Id);
        }
        finally {
            _dbUpdateSemaphore.Release();
        }
    }

    private async Task<CollectedAsset?> DownloadAssetWithSiteCollectorAsync(string url, CollectionTaskConfig? config, CancellationToken cancellationToken) {
        try {
            // 获取适合当前URL的站点采集器
            var siteCollector = _siteCollectorManager.GetCollectorForUrl(url);
            if (siteCollector != null) {
                return await siteCollector.DownloadAssetAsync(url, config, cancellationToken);
            }

            // 如果没有找到特定采集器，使用通用采集器
            var genericCollector = _siteCollectorManager.GetCollectorByIdentifier("generic");
            if (genericCollector != null) {
                return await genericCollector.DownloadAssetAsync(url, config, cancellationToken);
            }

            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No site collector found for URL: {Url}", url);
            return null;
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error downloading asset from {Url}", url);
            return null;
        }
    }

    private string PreprocessAssetUrl(string url, CollectionTaskConfig? config) {
        try {
            // 获取适合当前URL的站点采集器
            var siteCollector = _siteCollectorManager.GetCollectorForUrl(url);
            if (siteCollector != null) {
                return siteCollector.PreprocessAssetUrl(url, config);
            }

            // 如果没有找到特定采集器，使用通用采集器
            var genericCollector = _siteCollectorManager.GetCollectorByIdentifier("generic");
            if (genericCollector != null) {
                return genericCollector.PreprocessAssetUrl(url, config);
            }

            // 如果连通用采集器都找不到，返回原始URL
            return url;
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error preprocessing asset URL: {Url}", url);
            return url; // 返回原始URL作为后备
        }
    }

    private static List<string> ExtractAssetUrls(string htmlContent, string? selector) {
        // 简单的正则表达式匹配，实际应用中可能需要更复杂的HTML解析
        var urls = new List<string>();

        // 根据选择器类型提取URL
        if (selector?.StartsWith("img") is true) {
            // 提取图片URL
            var imgRegex = ImgRegex();
            var matches = imgRegex.Matches(htmlContent);
            foreach (Match match in matches) {
                var url = match.Groups["url"].Value;
                if (!string.IsNullOrEmpty(url)) {
                    urls.Add(url);
                }
            }
        }
        else if (selector?.StartsWith('a') is true) {
            // 提取链接URL
            var linkRegex = LinkRegex();
            var matches = linkRegex.Matches(htmlContent);
            foreach (Match match in matches) {
                var url = match.Groups["url"].Value;
                if (!string.IsNullOrEmpty(url)) {
                    urls.Add(url);
                }
            }
        }
        else {
            // 使用通用的选择器匹配
            var genericRegex = new Regex(selector ?? "a", RegexOptions.IgnoreCase);
            var matches = genericRegex.Matches(htmlContent);
            foreach (Match match in matches) {
                if (match.Groups.Count > 0) {
                    var url = match.Groups[0].Value;
                    if (!string.IsNullOrEmpty(url)) {
                        urls.Add(url);
                    }
                }
            }
        }

        return [.. urls.Distinct()];
    }

    private static string ResolveUrl(string baseUrl, string relativeUrl) {
        if (Uri.IsWellFormedUriString(relativeUrl, UriKind.Absolute)) {
            return relativeUrl;
        }

        var baseUri = new Uri(baseUrl);
        var resolvedUri = new Uri(baseUri, relativeUrl);
        return resolvedUri.ToString();
    }

    /// <summary>
    /// 默认的根据配置模式构造下一页URL方法
    /// </summary>
    private static string ConstructNextPageUrlByPatternDefault(string baseUrl, int pageNumber, string pattern) {
        // 使用配置中指定的模式来构造翻页URL
        // 模式可以包含占位符如 {page} 或 {pageNumber}
        var result = pattern.Replace("{page}", pageNumber.ToString())
                           .Replace("{pageNumber}", pageNumber.ToString());

        // 如果模式包含 {baseUrl} 占位符，替换为原始URL
        if (result.Contains("{baseUrl}")) {
            result = result.Replace("{baseUrl}", baseUrl);
        }

        // 如果模式是相对路径，将其附加到基础URL
        if (!Uri.IsWellFormedUriString(result, UriKind.Absolute)) {
            var baseUri = new Uri(baseUrl);
            var nextUri = new Uri(baseUri, result);
            return nextUri.ToString();
        }

        return result;
    }

    /// <summary>
    /// 默认的构造下一页URL方法
    /// </summary>
    private static string ConstructNextPageUrlDefault(string baseUrl, int pageNumber) {
        // 这里根据不同的网站模式构造翻页URL
        // 示例：
        // 1. ?page=2 或 ?p=2
        // 2. /page/2/
        // 3. /p2.html

        if (baseUrl.Contains('?')) {
            // 尝试添加或更新页码参数
            var uriBuilder = new UriBuilder(baseUrl);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            // 检查是否已存在页码参数
            if (query.AllKeys.Contains("page")) {
                query["page"] = pageNumber.ToString();
            }
            else if (query.AllKeys.Contains("p")) {
                query["p"] = pageNumber.ToString();
            }
            else {
                // 添加页码参数
                query.Add("page", pageNumber.ToString());
            }

            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }
        else {
            // 简单情况：在URL末尾添加页码
            if (baseUrl.EndsWith('/')) {
                return $"{baseUrl}page/{pageNumber}";
            }
            else {
                return $"{baseUrl}/page/{pageNumber}";
            }
        }
    }

    [GeneratedRegex(@"<img[^>]+src\s*=\s*[""'](?<url>[^""']+)[""']", RegexOptions.IgnoreCase, "zh-CN")]
    private static partial Regex ImgRegex();
    [GeneratedRegex(@"<a[^>]+href\s*=\s*[""'](?<url>[^""']+)[""']", RegexOptions.IgnoreCase, "zh-CN")]
    private static partial Regex LinkRegex();
}
