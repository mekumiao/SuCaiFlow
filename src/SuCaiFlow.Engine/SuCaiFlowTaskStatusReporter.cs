using Microsoft.Extensions.Logging;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowTaskStatusReporter(
    ILogger<SuCaiFlowTaskStatusReporter> logger,
    ISuCaiFlowTaskManager taskManager,
    ISuCaiFlowEngineEventPublisher publisher) {

    public async Task ReportPendingAsync(
        SuCaiFlowTaskContext ctx,
        CancellationToken ct = default) {
        ctx.Descriptor.MarkPending();
        await UpdateAsync(ctx, ct);
    }

    public async Task ReportRunningAsync(
        SuCaiFlowTaskContext ctx,
        CancellationToken ct = default) {
        ctx.Descriptor.MarkRunning();
        await UpdateAsync(ctx, ct);
        await publisher.PublishAsync<SuCaiFlowTaskStartedEvent>(new() {
            TaskId = ctx.TaskId,
            AssetsToCollectCount = ctx.Descriptor.AssetsToCollectCount,
        }, ct);
    }

    public async Task ReportProgressAsync(
        SuCaiFlowTaskContext ctx,
        CancellationToken ct = default) {
        await UpdateAsync(ctx, ct);
        await publisher.PublishAsync<SuCaiFlowTaskProgressEvent>(new() {
            TaskId = ctx.TaskId,
            AssetsCollectedCount = ctx.Descriptor.AssetsCollectedCount,
            AssetsToCollectCount = ctx.Descriptor.AssetsToCollectCount,
        }, ct);
    }

    public async Task ReportCompletedAsync(
        SuCaiFlowTaskContext ctx,
        CancellationToken ct = default) {
        ctx.Descriptor.MarkCompleted();
        await UpdateAsync(ctx, ct);
        ctx.Completion.TrySetResult();
        await publisher.PublishAsync<SuCaiFlowTaskCompletedEvent>(new() {
            TaskId = ctx.TaskId,
            AssetsCollectedCount = ctx.Descriptor.AssetsCollectedCount,
            AssetsToCollectCount = ctx.Descriptor.AssetsToCollectCount,
        }, ct);
    }

    public async Task ReportCanceledAsync(
        SuCaiFlowTaskContext ctx,
        CancellationToken ct = default) {
        ctx.Descriptor.MarkCanceled();
        await UpdateAsync(ctx, ct);
        ctx.Completion.TrySetCanceled(ctx.Cancellation.Token);
        await publisher.PublishAsync<SuCaiFlowTaskCanceledEvent>(new() {
            TaskId = ctx.TaskId,
            AssetsCollectedCount = ctx.Descriptor.AssetsCollectedCount,
            AssetsToCollectCount = ctx.Descriptor.AssetsToCollectCount,
        }, ct);
    }

    public async Task ReportFailedAsync(
        SuCaiFlowTaskContext ctx,
        Exception ex,
        CancellationToken ct = default) {
        ctx.Descriptor.MarkFailed(ex.Message);
        await UpdateAsync(ctx, ct);
        ctx.Completion.TrySetException(ex);
        await publisher.PublishAsync<SuCaiFlowTaskFailedEvent>(new() {
            TaskId = ctx.TaskId,
            ErrorMessage = ex.Message
        }, ct);
        logger.LogError(ex, "执行采集任务时出错 {taskId}", ctx.TaskId);
    }

    public async Task UpdateAsync(
        SuCaiFlowTaskContext ctx,
        CancellationToken ct = default) {
        var entity = await taskManager.FindByIdAsync(ctx.TaskId, ct);
        if (entity != null) {
            try {
                await taskManager.PopulateAsync(entity, ctx.Descriptor, ct);
                await taskManager.UpdateAsync(entity, ct);
            }
            catch (Exception ex) {
                logger.LogError(ex, "更新采集任务状态时出错 {taskId}", ctx.TaskId);
            }
        }
    }
}
