namespace SuCaiFlow.Contracts.Entities;

public class CollectionTask {
    public virtual Guid Id { get; set; }

    public virtual string? Name { get; set; }

    public virtual string? Description { get; set; }

    public virtual string? Url { get; set; }

    public virtual string? Selector { get; set; }

    public virtual CollectionTaskStatus Status { get; set; } = CollectionTaskStatus.Pending;

    public virtual DateTime? CreatedAt { get; set; }

    public virtual DateTime? StartedAt { get; set; }

    public virtual DateTime? CompletedAt { get; set; }

    public virtual int MaxConcurrency { get; set; }

    public virtual int AssetsCollectedCount { get; set; }

    public virtual int TotalAssetsExpected { get; set; }

    public virtual string? ErrorMessage { get; set; }

    public virtual Dictionary<string, string>? Parameters { get; set; }

    public virtual Guid? ConfigId { get; set; }

    public virtual CollectionTaskConfig? Config { get; set; }

    public virtual ICollection<CollectedAsset> Assets { get; private set; } = new HashSet<CollectedAsset>();

    public virtual void SetNewAssets(ICollection<CollectedAsset> assets) {
        Assets = assets;
    }
}
