using SuCaiFlow.Abstractions;
using SuCaiFlow.Abstractions.Descriptors;
using SuCaiFlow.Engine;

namespace SuCaiFlow.Core.Services;

public sealed class SuCaiFlowEngineService(
    ISuCaiFlowTaskManager flowTaskManager,
    SuCaiFlowTaskRegistry registry,
    SuCaiFlowTaskScheduler scheduler) {

    public async Task<SuCaiFlowTaskDescriptor> CreateAsync(
        SuCaiFlowTaskRequest request,
        CancellationToken ct = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SiteIdentifier);

        var descriptor = new SuCaiFlowTaskDescriptor {
            CreatedAt = DateTimeOffset.UtcNow,
            Status = SuCaiFlowConstants.TaskStatuses.Pending,
            ErrorMessage = default
        };
        descriptor.MapFrom(request);

        var entity = await flowTaskManager.CreateAsync(descriptor, ct);
        await flowTaskManager.PopulateAsync(descriptor, entity, ct);

        return descriptor;
    }

    public async Task StartAsync(string taskId, CancellationToken ct = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);

        var entity = await flowTaskManager.FindByIdAsync(taskId, ct)
            ?? throw new SuCaiFlowExceptions.NotFoundTaskException($"未找到任务: {taskId}");

        var descriptor = new SuCaiFlowTaskDescriptor();
        await flowTaskManager.PopulateAsync(descriptor, entity, ct);

        if (descriptor.Status == SuCaiFlowConstants.TaskStatuses.Completed)
            throw new InvalidOperationException("任务已完成，请新建任务");

        if (registry.TryRegister(descriptor, out var ctx))
            await scheduler.EnqueueAsync(ctx, ct);
    }

    public void Cancel(string taskId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);

        registry.Cancel(taskId);
    }
}
