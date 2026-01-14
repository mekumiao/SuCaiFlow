using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowEngineTaskExecutor {
    private readonly ActionBlock<SuCaiFlowTaskDescriptor> _flowTaskBlock;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISuCaiFlowEngineEventPublisher _eventPublisher;
    private readonly ISuCaiFlowEngineSiteCollectorManager _siteCollectorManager;
    private readonly SuCaiFlowEngineTaskTracker _tracker;
    private readonly ILogger<SuCaiFlowEngineTaskExecutor> _logger;
    private readonly SuCaiFlowEngineOptions _options;

    public SuCaiFlowEngineTaskExecutor(
        SuCaiFlowEngineTaskTracker tracker,
        IServiceScopeFactory scopeFactory,
        ISuCaiFlowEngineEventPublisher eventPublisher,
        ISuCaiFlowEngineSiteCollectorManager siteCollectorManager,
        IOptions<SuCaiFlowEngineOptions> options,
        ILogger<SuCaiFlowEngineTaskExecutor> logger,
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
                    _logger.LogError(ex, "Error rendering task {TaskId}", task.TaskId);
            }
        }, executionOptions);
    }

    public async Task EnqueueTaskAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        if (!_tracker.TryAdd(descriptor)) return;
        await _flowTaskBlock.SendAsync(descriptor, cancellationToken);
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default) {
        _flowTaskBlock.Complete();
        await _flowTaskBlock.Completion.WaitAsync(cancellationToken);
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
    }

    private async Task ExecuteParallelCollectionAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken) {
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.SiteIdentifier);
        ArgumentNullException.ThrowIfNull(_options);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(_options.MaxConcurrentDownloads);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_options.QueueCapacity, 500);

        var channel = Channel.CreateBounded<SuCaiFlowAssetDescriptor>(
            new BoundedChannelOptions(_options.MaxConcurrentDownloads * _options.QueueCapacity) {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = true,
                SingleReader = false
            });
        var collector = _siteCollectorManager.GetCollectorByIdentifier(descriptor.SiteIdentifier)
            ?? throw new InvalidOperationException($"未找到标识为 {descriptor.SiteIdentifier} 的采集站实现类 ISuCaiFlowEngineSiteCollector");

        try {
            var parsingTask = ParsePagesWithPaginationAsync(descriptor, channel, collector, cancellationToken);
            var downloadingTask = ExecuteAssetsDownloadAsync(channel, collector, cancellationToken);

            await Task.WhenAll(parsingTask, downloadingTask);
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error executing parallel collection for task {TaskId}", descriptor.TaskId);
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
        var collected = descriptor.AssetsCollectedCount;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var assetManager = scope.ServiceProvider.GetRequiredService<ISuCaiFlowAssetManager>();

        try {
            while (collected < descriptor.AssetsToCollectCount) {
                var assetDescriptors = await collector.ParsePageAsync(descriptor, currentPage, cancellationToken);

                if (assetDescriptors.Count == 0) break;

                foreach (var item in assetDescriptors) {
                    collector.ParseStorageName(item);
                    item.OrderNo = ++collected;
                    item.CreatedAt = DateTimeOffset.UtcNow;
                }

                await assetManager.CreateRangeAsync(assetDescriptors, cancellationToken);

                foreach (var item in assetDescriptors) {
                    await channel.Writer.WriteAsync(item, cancellationToken);
                }

                currentPage++;

                await Task.Delay(requestDelayMs, cancellationToken);
            }
        }
        catch (Exception ex) {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Error parsing pages for task {TaskId}", descriptor.TaskId);
            throw;
        }
        finally {
            descriptor.AssetsCollectedCount = collected;
            channel.Writer.TryComplete();
        }
    }

    private Task ExecuteAssetsDownloadAsync(
        Channel<SuCaiFlowAssetDescriptor> channel,
        ISuCaiFlowEngineSiteCollector collector,
        CancellationToken cancellationToken) {
        var tasks = new List<Task>();

        for (int i = 0; i < _options.MaxConcurrentDownloads; i++) {
            tasks.Add(Task.Run(async () => {
                await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken)) {
                    await collector.DownloadAssetAsync(item, cancellationToken);
                }
            }, cancellationToken));
        }

        return Task.WhenAll(tasks);
    }
}
