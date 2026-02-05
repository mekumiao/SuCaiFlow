using Microsoft.Extensions.Logging;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowTaskStatusReporter(
    ILogger<SuCaiFlowTaskStatusReporter> logger,
    ISuCaiFlowTaskManager taskManager,
    ISuCaiFlowEngineEventPublisher publisher) {

    public async Task ReportRunningAsync(
        SuCaiFlowTaskContext ctx,
        CancellationToken ct = default) {
        ctx.Descriptor.MarkRunning();
        await ClearnAssetsAsync(ctx, ct);
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
        ctx.Completion.TrySetResult();
        await UpdateAsync(ctx, ct);
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
        ctx.Completion.TrySetCanceled(ct);
        await UpdateAsync(ctx, ct);
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
        ctx.Completion.TrySetException(ex);
        await UpdateAsync(ctx, ct);
        await publisher.PublishAsync<SuCaiFlowTaskFailedEvent>(new() {
            TaskId = ctx.TaskId,
            ErrorMessage = ex.Message
        }, ct);
    }

    private async Task ClearnAssetsAsync(
        SuCaiFlowTaskContext ctx,
        CancellationToken ct = default) {
        if (ctx.Descriptor.AssetsCollectedCount > 0) {
            ctx.Descriptor.AssetsCollectedCount = 0;
            await taskManager.ClearAssetsAsync(ctx.TaskId, ct);
            await UpdateAsync(ctx, ct);
        }
    }

    private async Task UpdateAsync(
        SuCaiFlowTaskContext ctx,
        CancellationToken ct = default) {
        var entity = await taskManager.FindByIdAsync(ctx.TaskId, ct);
        if (entity != null) {
            await taskManager.PopulateAsync(entity, ctx.Descriptor, ct);
            try {
                await taskManager.UpdateAsync(entity, ct);
            }
            catch (Exception ex) {
                logger.LogError(ex, "更新采集任务状态时出错");
            }
        }
    }
}
