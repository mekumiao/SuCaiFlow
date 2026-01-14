using SuCaiFlow.Abstractions;
using SuCaiFlow.Engine;

namespace SuCaiFlow.Core.Services;

public class SuCaiFlowEngineService(ISuCaiFlowTaskManager flowTaskManager, SuCaiFlowEngineTaskExecutor executor) {
    private readonly ISuCaiFlowTaskManager _flowTaskManager = flowTaskManager;

    public Task CreateFlowTaskAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        return CreateFlowTaskAsync(descriptor, true, cancellationToken);
    }

    public async Task CreateFlowTaskAsync(SuCaiFlowTaskDescriptor descriptor, bool pushOnCreated, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.SiteIdentifier);

        descriptor.CreatedAt = DateTimeOffset.UtcNow;
        descriptor.Status = SuCaiFlowConstants.TaskStatuses.Pending;
        descriptor.ErrorMessage = default;

        var entity = await _flowTaskManager.CreateAsync(descriptor, cancellationToken);
        await _flowTaskManager.PopulateAsync(descriptor, entity, cancellationToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.TaskId);

        if (pushOnCreated) {
            await PushFlowTaskAsync(descriptor, cancellationToken);
        }
    }

    public async Task PushFlowTaskAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        await executor.EnqueueTaskAsync(descriptor, cancellationToken);
    }
}
