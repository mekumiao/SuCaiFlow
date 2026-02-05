using Microsoft.Extensions.Logging;

namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineDefaultEventPublisher(ILogger<SuCaiFlowEngineDefaultEventPublisher> logger) : ISuCaiFlowEngineEventPublisher {
    private readonly ILogger<SuCaiFlowEngineDefaultEventPublisher> _logger = logger;

    public async Task PublishAsync<TEvent>(TEvent e, CancellationToken ct = default) where TEvent : class {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("发布事件: {EventType}", typeof(TEvent).Name);

        await Task.CompletedTask;
    }
}
