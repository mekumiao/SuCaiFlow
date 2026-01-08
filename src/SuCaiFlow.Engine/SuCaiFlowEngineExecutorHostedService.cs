using Microsoft.Extensions.Hosting;

namespace SuCaiFlow.Engine;

internal sealed class SuCaiFlowEngineExecutorHostedService(SuCaiFlowEngineConcurrencyExecutor executor) : BackgroundService {
    private readonly SuCaiFlowEngineConcurrencyExecutor _executor = executor;

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        _executor.Start(stoppingToken);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken) {
        _executor.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
