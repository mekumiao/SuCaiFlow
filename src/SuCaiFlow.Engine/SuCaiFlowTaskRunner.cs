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
            await ParsePagesAndDownloadAsync(ctx, scheduler, reporter, CreateCollector(), assetManager);
            await reporter.ReportCompletedAsync(ctx);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) {
            await reporter.ReportCanceledAsync(ctx);
        }
        catch (Exception ex) {
            await reporter.ReportFailedAsync(ctx, ex);
        }
        finally {
            registry.Release(ctx.TaskId);
        }

        ISuCaiFlowEngineSiteCollector CreateCollector() {
            var siteId = ctx.Descriptor.SiteIdentifier;
            ArgumentException.ThrowIfNullOrWhiteSpace(siteId);
            return scope.ServiceProvider.GetRequiredKeyedService<ISuCaiFlowEngineSiteCollector>(siteId);
        }
    }

    public async Task RunAsync(SuCaiFlowDownloadTaskContext ctx) {
        var ct = ctx.CancellationToken;

        try {
            ct.ThrowIfCancellationRequested();
            await ctx.Collector.DownloadAssetAsync(ctx.AssetDescriptor, ct);
            ctx.Descriptor.DownloadIncrement();
            ctx.Completion.TrySetResult();
            await publisher.PublishAsync<SuCaiFlowTaskDownloadProgressEvent>(new() {
                TaskId = ctx.Descriptor.TaskId!,
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
        ISuCaiFlowEngineSiteCollector collector,
        ISuCaiFlowAssetManager assetManager) {
        int collected = 0;
        int currentPage = 0;
        var descriptor = ctx.Descriptor;
        var ct = ctx.Cancellation.Token;

        var completionChannel = Channel.CreateUnbounded<TaskCompletionSource>(new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = true,
        });

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
                        descriptor,
                        item,
                        collector,
                        ct);
                    await scheduler.EnqueueAsync(assetCtx, ct);
                    await completionChannel.Writer.WriteAsync(assetCtx.Completion, ct);
                }

                await Task.Delay(NextRequestDelayMilliseconds(), ct);
            }
        }
        finally {
            descriptor.AssetsCollectedCount = collected;
            completionChannel.Writer.TryComplete();
            await completionChannelTask;
        }
    }

    private int NextRequestDelayMilliseconds() {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(_options.RequestDelayMs);

        var minMilliseconds = _options.RequestDelayMs;
        var maxMilliseconds = _options.RequestDelayMs + 2000;
        return Random.Shared.Next(minMilliseconds, maxMilliseconds + 1);
    }
}
