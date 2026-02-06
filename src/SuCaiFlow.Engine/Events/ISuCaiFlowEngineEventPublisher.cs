namespace SuCaiFlow.Engine;

public interface ISuCaiFlowEngineEventPublisher {
    Task PublishAsync<TEvent>(TEvent e, CancellationToken ct = default) where TEvent : class;
}
