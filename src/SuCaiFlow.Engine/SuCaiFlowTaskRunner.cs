using System.Threading.Channels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowTaskRunner(
    ILogger<SuCaiFlowTaskRunner> logger,
    IOptions<SuCaiFlowEngineOptions> options,
    IServiceScopeFactory scopeFactory,
    ISuCaiFlowEngineSiteCollectorManager collectorManager,
    ISuCaiFlowEngineEventPublisher publisher,
    SuCaiFlowTaskRegistry registry) {
    private readonly SuCaiFlowEngineOptions _options = options.Value;

    public async Task RunAsync(SuCaiFlowTaskContext ctx) {
        await using var scope = scopeFactory.CreateAsyncScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<SuCaiFlowTaskScheduler>();
        var reporter = scope.ServiceProvider.GetRequiredService<SuCaiFlowTaskStatusReporter>();
        var assetManager = scope.ServiceProvider.GetRequiredService<ISuCaiFlowAssetManager>();
        var ct = ctx.Cancellation.Token;

        try {
            ct.ThrowIfCancellationRequested();
            await reporter.ReportRunningAsync(ctx);
            await ParsePagesAndDownloadAsync(ctx, scheduler, reporter, assetManager);
            await reporter.ReportCompletedAsync(ctx);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) {
            await reporter.ReportCanceledAsync(ctx);
        }
        catch (Exception ex) {
            await reporter.ReportFailedAsync(ctx, ex);
        }
        finally {
            registry.Complete(ctx.TaskId);
        }
    }

    public async Task RunAsync(SuCaiFlowDownloadTaskContext ctx) {
        var ct = ctx.CancellationToken;
        if (ct.IsCancellationRequested) return;

        try {
            await ctx.Collector.DownloadAssetAsync(ctx.AssetDescriptor, ct);
            ctx.Descriptor.DownloadIncrement();
            ctx.Completion.TrySetResult();
            await publisher.PublishAsync<SuCaiFlowTaskDownloadProgressEvent>(new() {
                TaskId = ctx.TaskId,
                AssetsDownloadCount = ctx.Descriptor.AssetsDownloadCount,
                AssetsToCollectCount = ctx.Descriptor.AssetsToCollectCount,
            }, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) {
            ctx.Completion.TrySetCanceled(ct);
        }
        catch (Exception ex) {
            ctx.Completion.TrySetException(ex);
            logger.LogError(ex, "下载图片时出错 {url}", ctx.AssetDescriptor.OriginalUrl);
        }
    }

    private async Task ParsePagesAndDownloadAsync(
        SuCaiFlowTaskContext ctx,
        SuCaiFlowTaskScheduler scheduler,
        SuCaiFlowTaskStatusReporter reporter,
        ISuCaiFlowAssetManager assetManager) {
        int collected = 0;
        int currentPage = 0;
        var descriptor = ctx.Descriptor;
        var siteId = ctx.Descriptor.SiteIdentifier;
        var ct = ctx.Cancellation.Token;
        var downloadQueueCapacity = _options.DownloadQueueCapacity;

        ArgumentException.ThrowIfNullOrWhiteSpace(siteId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(downloadQueueCapacity);

        var collector = collectorManager.GetCollectorByIdentifier(siteId)
            ?? throw new InvalidOperationException($"未找到标识为 {siteId} 的采集站实现类");
        var contextChannel = Channel.CreateBounded<SuCaiFlowDownloadTaskContext>(new BoundedChannelOptions(downloadQueueCapacity) {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
        });
        var completionChannel = Channel.CreateBounded<TaskCompletionSource>(new BoundedChannelOptions(downloadQueueCapacity) {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
        });

        var contextChannelTask = Task.Run(async () => {
            await foreach (var item in contextChannel.Reader.ReadAllAsync(ct)) {
                await scheduler.EnqueueAsync(item, ct);
            }
        }, ct);
        var completionChannelTask = Task.Run(async () => {
            await foreach (var item in completionChannel.Reader.ReadAllAsync(ct)) {
                try { await item.Task.WaitAsync(ct); } catch { }
            }
        }, ct);

        try {
            while (collected < descriptor.AssetsToCollectCount) {
                ct.ThrowIfCancellationRequested();

                var assetDescriptors = await collector.ParsePageAsync(descriptor, ++currentPage, ct);
                if (assetDescriptors.Count == 0) break;

                var now = DateTimeOffset.UtcNow;
                foreach (var item in assetDescriptors) {
                    item.ObjectKey = collector.ParseObjectKey(item);
                    item.OrderNo = ++collected;
                    item.CreatedAt = now;
                }

                descriptor.AssetsCollectedCount = collected;
                await assetManager.CreateRangeAsync(assetDescriptors, ct);
                await reporter.ReportProgressAsync(ctx);

                foreach (var item in assetDescriptors) {
                    var assetCtx = new SuCaiFlowDownloadTaskContext(
                        ctx.TaskId,
                        descriptor,
                        item,
                        collector,
                        ct);
                    await contextChannel.Writer.WriteAsync(assetCtx, ct);
                    await completionChannel.Writer.WriteAsync(assetCtx.Completion, ct);
                }

                await Task.Delay(NextRequestDelayMilliseconds(), ct);
            }
        }
        finally {
            descriptor.AssetsCollectedCount = collected;
            contextChannel.Writer.TryComplete();
            completionChannel.Writer.TryComplete();
            await contextChannelTask;
            await completionChannelTask;
        }
    }

    public int NextRequestDelayMilliseconds() {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(_options.RequestDelayMs);

        var minMilliseconds = _options.RequestDelayMs;
        var maxMilliseconds = _options.RequestDelayMs + 2000;
        return Random.Shared.Next(minMilliseconds, maxMilliseconds + 1);
    }
}
