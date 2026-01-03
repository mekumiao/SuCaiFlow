namespace SuCaiFlow.Contracts.Services;

/// <summary>
/// 事件发布器接口，用于发布各种采集事件
/// </summary>
public interface IEventPublisher {
    /// <summary>
    /// 发布事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="event">事件实例</param>
    /// <param name="cancellationToken"></param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : class;
}
