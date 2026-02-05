using Microsoft.Extensions.Hosting;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowEngineHostedService(SuCaiFlowTaskScheduler scheduler) : IHostedService {
    public Task StartAsync(CancellationToken cancellationToken) {
        //throw new NotImplementedException();
        // todo: 恢复任务
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        await scheduler.StopAsync();
    }
}
