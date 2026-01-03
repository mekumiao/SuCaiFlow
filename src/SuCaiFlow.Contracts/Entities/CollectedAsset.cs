namespace SuCaiFlow.Contracts.Entities;

public class CollectedAsset {
    public virtual Guid Id { get; set; }

    public virtual string? Name { get; set; }

    public virtual string? Description { get; set; }

    public virtual string? Url { get; set; }

    public virtual string? LocalPath { get; set; }

    public virtual string? ContentType { get; set; }

    public virtual long Size { get; set; }

    public virtual Guid CollectionTaskId { get; set; }

    public virtual CollectionTask? CollectionTask { get; set; }

    public virtual DateTime? CreatedAt { get; set; }

    public virtual DateTime? DownloadedAt { get; set; }

    public virtual CollectedAssetStatus Status { get; set; } = CollectedAssetStatus.Pending;

    public virtual string? ErrorMessage { get; set; }
}

public enum CollectedAssetStatus {
    Pending = 0,      // 待采集
    InProgress = 1,   // 采集中
    Completed = 2,    // 已完成
    Failed = 3        // 采集失败
}
