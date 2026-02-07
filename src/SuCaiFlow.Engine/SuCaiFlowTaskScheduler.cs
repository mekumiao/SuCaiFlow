using System.Threading.Tasks.Dataflow;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowTaskScheduler {
    private readonly ActionBlock<SuCaiFlowTaskContext> _block;
    private readonly ActionBlock<SuCaiFlowDownloadTaskContext> _dblock;

    public SuCaiFlowTaskScheduler(
        SuCaiFlowTaskRunner runner,
        IOptions<SuCaiFlowEngineOptions> options,
        IHostApplicationLifetime lifetime) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.Value.MaxConcurrency);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.Value.QueueCapacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.Value.MaxDownloadConcurrency);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.Value.DownloadQueueCapacity);

        _block = new ActionBlock<SuCaiFlowTaskContext>(
            runner.RunAsync,
            new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = options.Value.MaxConcurrency,
                BoundedCapacity = options.Value.QueueCapacity,
                EnsureOrdered = false,
                CancellationToken = lifetime.ApplicationStopping,
            });

        _dblock = new ActionBlock<SuCaiFlowDownloadTaskContext>(
            runner.RunAsync,
            new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = options.Value.MaxDownloadConcurrency,
                BoundedCapacity = options.Value.DownloadQueueCapacity,
                EnsureOrdered = false,
                CancellationToken = lifetime.ApplicationStopping,
            });
    }

    public async Task EnqueueAsync(SuCaiFlowTaskContext ctx, CancellationToken ct = default) {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ctx.Cancellation.Token, ct);
        await _block.SendAsync(ctx, cts.Token);
    }

    public async Task EnqueueAsync(SuCaiFlowDownloadTaskContext ctx, CancellationToken ct = default) {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ctx.CancellationToken, ct);
        await _dblock.SendAsync(ctx, cts.Token);
    }

    public async Task StopAsync() {
        _block.Complete();
        _dblock.Complete();
        try { await _block.Completion; } catch { }
        try { await _dblock.Completion; } catch { }
    }
}
