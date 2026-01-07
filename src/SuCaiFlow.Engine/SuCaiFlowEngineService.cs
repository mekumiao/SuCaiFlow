using System.Threading.Channels;

using Microsoft.Extensions.Logging;

using SuCaiFlow.Abstractions;
using SuCaiFlow.Engine;

namespace SuCaiFlow.Core.Services;

public partial class SuCaiFlowEngineService(
    ILogger<SuCaiFlowEngineService> logger,
    ISuCaiFlowEngineSiteCollectorManager siteCollectorManager,
    ISuCaiFlowEngineEventPublisher eventPublisher,
    SuCaiFlowEngineConcurrencyExecutor executor,
    SuCaiFlowEngineTaskTracker tracker) {
    private readonly ILogger<SuCaiFlowEngineService> _logger = logger;
    private readonly ISuCaiFlowEngineSiteCollectorManager _siteCollectorManager = siteCollectorManager;
    private readonly ISuCaiFlowEngineEventPublisher _eventPublisher = eventPublisher;
    private readonly SuCaiFlowEngineConcurrencyExecutor _executor = executor;
    private readonly SuCaiFlowEngineTaskTracker _tracker = tracker;

    public async Task StartFlowTaskAsync(string taskId, CancellationToken cancellationToken) {
        var (created, descriptor) = await _tracker.TryCreateAsync(taskId, async taskId => {
            var task = new SuCaiFlowTaskDescriptor {
                TaskId = taskId,
                TotalAssetsExpected = 100,
                CreatedAt = DateTime.UtcNow,
                Status = SuCaiFlowConstants.TaskStatuses.Pending,
                SiteIdentifier = "StockPhoto",
                SearchKeywords = "cut",
            };
            return task;
        });

        if (!created) return;

        ArgumentNullException.ThrowIfNull(descriptor);

        await _executor.EnqueueAsync(async token => {
            try {
                _tracker.MarkRunning(taskId);

                await _eventPublisher.PublishAsync(new SuCaiFlowTaskStartedEvent {
                    TaskId = taskId,
                }, cancellationToken);

                var assets = await ExecuteParallelCollectionAsync(descriptor, cancellationToken);

                descriptor.AssetsCollectedCount = assets.Count;

                _tracker.MarkCompleted(taskId);

                await _eventPublisher.PublishAsync(new SuCaiFlowTaskCompletedEvent {
                    TaskId = taskId,
                    AssetsCollectedCount = assets.Count,
                    TotalAssetsExpected = descriptor.TotalAssetsExpected,
                    Status = descriptor.Status
                }, cancellationToken);

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Completed collection task {TaskId}, collected {AssetCount} assets", taskId, assets.Count);
            }
            catch (OperationCanceledException) {
                _tracker.MarkCanceled(taskId);
                await _eventPublisher.PublishAsync(new SuCaiFlowTaskCanceledEvent {
                    TaskId = taskId,
                    AssetsCollectedCount = descriptor.AssetsCollectedCount,
                    TotalAssetsExpected = descriptor.TotalAssetsExpected,
                }, cancellationToken);
            }
            catch (Exception ex) {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(ex, "Error processing collection task {TaskId}", taskId);

                _tracker.MarkFailed(taskId, ex.Message);

                await _eventPublisher.PublishAsync(new SuCaiFlowTaskFailedEvent {
                    TaskId = taskId,
                    ErrorMessage = ex.Message
                }, cancellationToken);
            }
        }, cancellationToken);
    }

    private async Task<List<SuCaiFlowAssetDescriptor>> ExecuteParallelCollectionAsync(
        SuCaiFlowTaskDescriptor task,
        CancellationToken cancellationToken) {
        ArgumentException.ThrowIfNullOrWhiteSpace(task.SiteIdentifier);

        var semaphore = new SemaphoreSlim(10);
        var channel = Channel.CreateBounded<SuCaiFlowAssetDescriptor>(1000);
        var collector = _siteCollectorManager.GetCollectorByIdentifier(task.SiteIdentifier)
            ?? throw new InvalidOperationException($"未找到标识为 {task.SiteIdentifier} 的采集站实现类 ISuCaiFlowEngineSiteCollector");

        try {
            var parsingTask = Task.Run(async () => await ParsePagesWithPaginationAsync(
                task,
                channel,
                collector,
                cancellationToken), cancellationToken);
            var downloadingTask = Task.Run(async () => await ProcessAssetsDownloadsAsync(
                channel,
                collector,
                semaphore,
                cancellationToken), cancellationToken);

            await Task.WhenAll(parsingTask, downloadingTask);

            return parsingTask.Result;
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error executing parallel collection for task {TaskId}", task.TaskId);
            throw;
        }
    }

    private async Task<List<SuCaiFlowAssetDescriptor>> ParsePagesWithPaginationAsync(
        SuCaiFlowTaskDescriptor descriptor,
        Channel<SuCaiFlowAssetDescriptor> channel,
        ISuCaiFlowEngineSiteCollector collector,
        CancellationToken cancellationToken) {
        var currentPage = 1;
        var requestDelayMs = 1000;
        var list = new List<SuCaiFlowAssetDescriptor>();

        try {
            while (descriptor.AssetsCollectedCount < descriptor.TotalAssetsExpected) {
                var assetDescriptors = await collector.ParsePageAsync(descriptor, currentPage, cancellationToken);

                if (!assetDescriptors.Any()) break;

                foreach (var item in assetDescriptors) {
                    list.Add(item);
                    await channel.Writer.WriteAsync(item, cancellationToken);
                    descriptor.AssetsCollectedCount++;
                }

                currentPage++;

                await Task.Delay(requestDelayMs, cancellationToken);
            }
            channel.Writer.Complete();
            return list;
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error parsing pages for task {TaskId}", descriptor.TaskId);
            throw;
        }
    }

    private static async Task ProcessAssetsDownloadsAsync(
        Channel<SuCaiFlowAssetDescriptor> channel,
        ISuCaiFlowEngineSiteCollector collector,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken) {
        await Task.Run(async () => {
            await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken)) {
                try {
                    await semaphore.WaitAsync(cancellationToken);
                    await collector.DownloadAssetAsync(item, cancellationToken);
                }
                finally {
                    semaphore.Release();
                }
            }
        }, cancellationToken);
    }
}
