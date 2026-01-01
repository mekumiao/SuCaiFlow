using Microsoft.Extensions.Logging;

using SuCaiFlow.Contracts.Interfaces;

namespace SuCaiFlow.Core.Services;

/// <summary>
/// 事件发布器实现
/// </summary>
public class EventPublisher(ILogger<EventPublisher> logger) : IEventPublisher {
    private readonly ILogger<EventPublisher> _logger = logger;

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("发布事件: {EventType}", typeof(TEvent).Name);

        // 这里可以集成实际的事件总线或消息队列
        // 暂时只记录日志
        await Task.CompletedTask;
    }
}
