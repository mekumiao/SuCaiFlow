using System.Threading.Channels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SuCaiFlow.Abstractions;
using SuCaiFlow.Engine;

namespace SuCaiFlow.Core.Services;

public partial class SuCaiFlowEngineService(
    ILogger<SuCaiFlowEngineService> logger,
    ISuCaiFlowEngineSiteCollectorManager siteCollectorManager,
    ISuCaiFlowEngineEventPublisher eventPublisher,
    IServiceScopeFactory scopeFactory,
    SuCaiFlowEngineConcurrencyExecutor executor,
    SuCaiFlowEngineTaskTracker tracker) {
    private readonly ILogger<SuCaiFlowEngineService> _logger = logger;
    private readonly ISuCaiFlowEngineSiteCollectorManager _siteCollectorManager = siteCollectorManager;
    private readonly ISuCaiFlowEngineEventPublisher _eventPublisher = eventPublisher;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly SuCaiFlowEngineConcurrencyExecutor _executor = executor;
    private readonly SuCaiFlowEngineTaskTracker _tracker = tracker;

    public Task CreateFlowTaskAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        return CreateFlowTaskAsync(descriptor, true, cancellationToken);
    }

    public async Task CreateFlowTaskAsync(SuCaiFlowTaskDescriptor descriptor, bool pushOnCreated, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.SiteIdentifier);

        descriptor.Status = SuCaiFlowConstants.TaskStatuses.Pending;
        descriptor.ErrorMessage = default;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var flowTaskManager = scope.ServiceProvider.GetRequiredService<ISuCaiFlowTaskManager>();

        var entity = await flowTaskManager.CreateAsync(descriptor, cancellationToken);
        await flowTaskManager.PopulateAsync(descriptor, entity, cancellationToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.TaskId);

        if (pushOnCreated) {
            await PushFlowTaskAsync(descriptor, cancellationToken);
        }
    }

    public async Task PushFlowTaskAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.TaskId);

        if (!_tracker.TryAdd(descriptor)) return;

        await _executor.EnqueueAsync(async token => {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var taskManager = scope.ServiceProvider.GetRequiredService<ISuCaiFlowTaskManager>();

            var taskEntity = await taskManager.FindByIdAsync(descriptor.TaskId, cancellationToken);
            ArgumentNullException.ThrowIfNull(taskEntity);

            try {
                _tracker.MarkRunning(descriptor.TaskId);
                await taskManager.UpdateAsync(taskEntity, descriptor, cancellationToken);

                await _eventPublisher.PublishAsync(new SuCaiFlowTaskStartedEvent {
                    TaskId = descriptor.TaskId,
                }, cancellationToken);

                await ExecuteParallelCollectionAsync(descriptor, cancellationToken);

                _tracker.MarkCompleted(descriptor.TaskId);
                await taskManager.UpdateAsync(taskEntity, descriptor, cancellationToken);

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
                await taskManager.UpdateAsync(taskEntity, descriptor, cancellationToken);

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
                await taskManager.UpdateAsync(taskEntity, descriptor, cancellationToken);

                await _eventPublisher.PublishAsync(new SuCaiFlowTaskFailedEvent {
                    TaskId = descriptor.TaskId,
                    ErrorMessage = ex.Message
                }, cancellationToken);
            }
        }, cancellationToken);
    }

    private async Task ExecuteParallelCollectionAsync(
        SuCaiFlowTaskDescriptor task,
        CancellationToken cancellationToken) {
        ArgumentException.ThrowIfNullOrWhiteSpace(task.SiteIdentifier);

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
                cancellationToken), cancellationToken);

            await Task.WhenAll(parsingTask, downloadingTask);
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error executing parallel collection for task {TaskId}", task.TaskId);
            throw;
        }
    }

    private async Task ParsePagesWithPaginationAsync(
        SuCaiFlowTaskDescriptor descriptor,
        Channel<SuCaiFlowAssetDescriptor> channel,
        ISuCaiFlowEngineSiteCollector collector,
        CancellationToken cancellationToken) {
        var currentPage = 1;
        var requestDelayMs = 1000;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var assetManager = scope.ServiceProvider.GetRequiredService<ISuCaiFlowAssetManager>();

        try {
            while (descriptor.AssetsCollectedCount < descriptor.AssetsToCollectCount) {
                var assetDescriptors = await collector.ParsePageAsync(descriptor, currentPage, cancellationToken);

                if (!assetDescriptors.Any()) break;

                await assetManager.CreateRangeAsync(assetDescriptors, cancellationToken);

                foreach (var item in assetDescriptors) {
                    await channel.Writer.WriteAsync(item, cancellationToken);
                    descriptor.AssetsCollectedCount++;
                }

                currentPage++;

                await Task.Delay(requestDelayMs, cancellationToken);
            }
            channel.Writer.Complete();
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
        CancellationToken cancellationToken) {
        var semaphore = new SemaphoreSlim(10);
        await Task.Run(async () => {
            await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken)) {
                await semaphore.WaitAsync(cancellationToken);
                try {
                    await collector.DownloadAssetAsync(item, cancellationToken);
                }
                finally {
                    semaphore.Release();
                }
            }
        }, cancellationToken);
    }
}
