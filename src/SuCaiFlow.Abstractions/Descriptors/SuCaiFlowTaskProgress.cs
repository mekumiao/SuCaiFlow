namespace SuCaiFlow.Abstractions;

public sealed class SuCaiFlowTaskProgress(int assetsToCollectCount) {
    /// <summary>
    /// 当前已解析到的页码（从 1 开始）
    /// 用于重启恢复 ParsePageAsync
    /// </summary>
    public int CurrentPage { get; private set; } = 1;

    /// <summary>
    /// 已成功入库（或确认）的资源数量
    /// 用于恢复 collected 状态
    /// </summary>
    public int AssetsCollectedCount { get; private set; }

    /// <summary>
    /// 任务总目标资源数（快照）
    /// </summary>
    public int AssetsToCollectCount { get; } = assetsToCollectCount;

    /// <summary>
    /// 上一次进度持久化时间
    /// </summary>
    public DateTimeOffset LastCheckpointAt { get; private set; }
        = DateTimeOffset.UtcNow;

    /// <summary>
    /// 最近一次错误（可选）
    /// </summary>
    public string? LastError { get; private set; }

    private readonly int _checkpointIntervalPages = 1;
    private readonly TimeSpan _checkpointIntervalTime = TimeSpan.FromSeconds(5);

    public void MoveToNextPage() {
        CurrentPage++;
    }

    public void AddCollected(int count) {
        AssetsCollectedCount += count;
    }

    public void MarkError(Exception ex) {
        LastError = ex.Message;
    }

    public bool ShouldCheckpoint() {
        if (DateTimeOffset.UtcNow - LastCheckpointAt >= _checkpointIntervalTime)
            return true;

        if (CurrentPage % _checkpointIntervalPages == 0)
            return true;

        return false;
    }

    public void MarkCheckpointed() {
        LastCheckpointAt = DateTimeOffset.UtcNow;
    }
}
