using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public interface ISuCaiFlowEngineTaskExecutor {
    Task EnqueueTaskAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default);
}
