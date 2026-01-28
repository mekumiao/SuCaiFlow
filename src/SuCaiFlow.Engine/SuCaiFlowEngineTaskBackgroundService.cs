using System.Threading.Tasks.Dataflow;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowEngineTaskBackgroundService : BackgroundService, ISuCaiFlowEngineTaskExecutor {
    private readonly ActionBlock<SuCaiFlowTaskDescriptor> _flowTaskBlock;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISuCaiFlowEngineEventPublisher _eventPublisher;
    private readonly ISuCaiFlowEngineSiteCollectorManager _siteCollectorManager;
    private readonly SuCaiFlowEngineTaskTracker _tracker;
    private readonly ILogger<SuCaiFlowEngineTaskBackgroundService> _logger;
    private readonly SuCaiFlowEngineOptions _options;

    public SuCaiFlowEngineTaskBackgroundService(
        SuCaiFlowEngineTaskTracker tracker,
        IServiceScopeFactory scopeFactory,
        ISuCaiFlowEngineEventPublisher eventPublisher,
        ISuCaiFlowEngineSiteCollectorManager siteCollectorManager,
        IOptions<SuCaiFlowEngineOptions> options,
        ILogger<SuCaiFlowEngineTaskBackgroundService> logger,
        IHostApplicationLifetime lifetime) {
        _tracker = tracker;
        _scopeFactory = scopeFactory;
        _eventPublisher = eventPublisher;
        _siteCollectorManager = siteCollectorManager;
        _logger = logger;
        _options = options.Value;

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(_options.MaxConcurrency);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_options.QueueCapacity, 500);

        var executionOptions = new ExecutionDataflowBlockOptions {
            MaxDegreeOfParallelism = _options.MaxConcurrency,
            BoundedCapacity = _options.QueueCapacity,
            EnsureOrdered = false,
            CancellationToken = lifetime.ApplicationStopping
        };

        _flowTaskBlock = new ActionBlock<SuCaiFlowTaskDescriptor>(async task => {
            try {
                await ExecuteFlowTaskAsync(task, lifetime.ApplicationStopping);
            }
            catch (Exception ex) {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(ex, "Error FlowTask task {TaskId}", task.TaskId);
            }
        }, executionOptions);
    }

    public async Task EnqueueTaskAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        if (!_tracker.TryAdd(descriptor)) return;
        await _flowTaskBlock.SendAsync(descriptor, cancellationToken);
    }

    private static async Task UpdateTaskAsync(ISuCaiFlowTaskManager taskManager, SuCaiFlowTaskDescriptor descriptor, CancellationToken ct) {
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.TaskId);

        var entity = await taskManager.FindByIdAsync(descriptor.TaskId, ct);
        if (entity == null) return;

        await taskManager.UpdateAsync(entity, descriptor, ct);
    }

    private async Task ExecuteFlowTaskAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.TaskId);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var taskManager = scope.ServiceProvider.GetRequiredService<ISuCaiFlowTaskManager>();

        var taskEntity = await taskManager.FindByIdAsync(descriptor.TaskId, cancellationToken);
        ArgumentNullException.ThrowIfNull(taskEntity);

        try {
            _tracker.MarkRunning(descriptor.TaskId);
            if (descriptor.AssetsCollectedCount > 0) {
                await taskManager.ClearAssetsAsync(descriptor.TaskId, cancellationToken);
                descriptor.AssetsCollectedCount = 0;
            }
            await UpdateTaskAsync(taskManager, descriptor, cancellationToken);
            //await taskManager.UpdateAsync(taskEntity, descriptor, cancellationToken);

            await _eventPublisher.PublishAsync(new SuCaiFlowTaskStartedEvent {
                TaskId = descriptor.TaskId,
            }, cancellationToken);

            await ExecuteParallelCollectionAsync(descriptor, cancellationToken);

            _tracker.MarkCompleted(descriptor.TaskId);
            //await taskManager.UpdateAsync(taskEntity, descriptor, cancellationToken);
            await UpdateTaskAsync(taskManager, descriptor, cancellationToken);

            await _eventPublisher.PublishAsync(new SuCaiFlowTaskCompletedEvent {
                TaskId = descriptor.TaskId,
                AssetsCollectedCount = descriptor.AssetsCollectedCount,
                TotalAssetsExpected = descriptor.AssetsToCollectCount,
                Status = descriptor.Status
            }, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Completed collection task {TaskId}, collected {AssetCount} assets", descriptor.TaskId, descriptor.AssetsCollectedCount);
        }
        catch (OperationCanceledException) {
            _tracker.MarkCanceled(descriptor.TaskId);
            //await taskManager.UpdateAsync(taskEntity, descriptor, cancellationToken);
            await UpdateTaskAsync(taskManager, descriptor, cancellationToken);

            await _eventPublisher.PublishAsync(new SuCaiFlowTaskCanceledEvent {
                TaskId = descriptor.TaskId,
                AssetsCollectedCount = descriptor.AssetsCollectedCount,
                TotalAssetsExpected = descriptor.AssetsToCollectCount,
            }, cancellationToken);
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error processing collection task {TaskId}", descriptor.TaskId);

            _tracker.MarkFailed(descriptor.TaskId, ex.Message);
            //await taskManager.UpdateAsync(taskEntity, descriptor, cancellationToken);
            await UpdateTaskAsync(taskManager, descriptor, cancellationToken);

            await _eventPublisher.PublishAsync(new SuCaiFlowTaskFailedEvent {
                TaskId = descriptor.TaskId,
                ErrorMessage = ex.Message
            }, cancellationToken);
        }
    }

    private async Task ExecuteParallelCollectionAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken) {
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.SiteIdentifier);
        ArgumentNullException.ThrowIfNull(_options);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(_options.MaxDownloadConcurrency);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_options.QueueCapacity, 500);

        var collector = _siteCollectorManager.GetCollectorByIdentifier(descriptor.SiteIdentifier)
            ?? throw new InvalidOperationException($"未找到标识为 {descriptor.SiteIdentifier} 的采集站实现类 ISuCaiFlowEngineSiteCollector");

        var downloadBlackOptions = new ExecutionDataflowBlockOptions {
            MaxDegreeOfParallelism = _options.MaxDownloadConcurrency,
            BoundedCapacity = _options.MaxDownloadConcurrency * _options.QueueCapacity,
            EnsureOrdered = false,
            CancellationToken = cancellationToken
        };

        var downloadBlock = new ActionBlock<SuCaiFlowAssetDescriptor>(async assetDescriptor => {
            try {
                await collector.DownloadAssetAsync(assetDescriptor, cancellationToken);
            }
            catch (Exception ex) {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(ex, "Error downloading asset {OriginalUrl} for task {TaskId}", assetDescriptor.OriginalUrl, descriptor.TaskId);
            }
        }, downloadBlackOptions);

        await ParsePagesAndDownloadAsync(descriptor, downloadBlock, collector, cancellationToken);
    }

    private async Task ParsePagesAndDownloadAsync(SuCaiFlowTaskDescriptor descriptor, ActionBlock<SuCaiFlowAssetDescriptor> downloadBlock, ISuCaiFlowEngineSiteCollector collector, CancellationToken cancellationToken) {
        var currentPage = 1;
        var requestDelayMs = 1000;
        var collected = descriptor.AssetsCollectedCount;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var assetManager = scope.ServiceProvider.GetRequiredService<ISuCaiFlowAssetManager>();

        try {
            while (collected < descriptor.AssetsToCollectCount) {
                var assetDescriptors = await collector.ParsePageAsync(descriptor, currentPage, cancellationToken);

                if (assetDescriptors.Count == 0) break;

                foreach (var item in assetDescriptors) {
                    collector.ParseObjectKey(item);
                    item.OrderNo = ++collected;
                    item.CreatedAt = DateTimeOffset.UtcNow;
                }

                await assetManager.CreateRangeAsync(assetDescriptors, cancellationToken);

                foreach (var item in assetDescriptors) {
                    await downloadBlock.SendAsync(item, cancellationToken);
                }

                currentPage++;

                await Task.Delay(requestDelayMs, cancellationToken);
            }
        }
        finally {
            descriptor.AssetsCollectedCount = collected;
            downloadBlock.Complete();
            await downloadBlock.Completion;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) {
        }
        finally {
            _flowTaskBlock.Complete();
            await _flowTaskBlock.Completion;
        }
    }
}
