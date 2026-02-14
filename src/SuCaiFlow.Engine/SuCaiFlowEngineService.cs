using Microsoft.Extensions.Logging;

using SuCaiFlow.Abstractions;
using SuCaiFlow.Engine;

namespace SuCaiFlow.Core.Services;

public sealed class SuCaiFlowEngineService(
    ILogger<SuCaiFlowEngineService> logger,
    ISuCaiFlowTaskManager flowTaskManager,
    SuCaiFlowTaskRegistry registry,
    SuCaiFlowTaskStatusReporter reporter,
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

#if !DEBUG
        if (descriptor.Status == SuCaiFlowConstants.TaskStatuses.Completed)
            throw new InvalidOperationException("任务已完成，请新建任务");
#endif

        if (registry.TryRegister(descriptor, out var ctx))
            try {
                ctx.Descriptor.AssetsCollectedCount = 0;
                await flowTaskManager.ClearAssetsAsync(ctx.TaskId, ct);
                await reporter.ReportPendingAsync(ctx, ct);
                await scheduler.EnqueueAsync(ctx, ct);
            }
            catch (Exception ex) {
                registry.Release(ctx.TaskId);
                await reporter.ReportFailedAsync(ctx, ex, default);
                logger.LogError(ex, "采集任务入队时失败 {taskId}", ctx.TaskId);
            }
    }

    public async Task DeleteAsync(string taskId, CancellationToken ct = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);

        registry.Cancel(taskId);
        var entity = await flowTaskManager.FindByIdAsync(taskId, ct);
        if (entity != null) await flowTaskManager.DeleteAsync(entity, ct);
    }

    public async Task CancelAndWaitForCompletionAsync(string taskId, CancellationToken ct = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);

        await registry.CancelAndWaitForCompletionAsync(taskId, ct);

        var entity = await flowTaskManager.FindByIdAsync(taskId, ct);
        if (entity != null && !await flowTaskManager.GetKeepAfterCancelAsync(entity, ct)) {
            await flowTaskManager.DeleteAsync(entity, ct);
        }
    }
}
