namespace SuCaiFlow.EntityFrameworkCore.Models;

public class SuCaiFlowEntityFrameworkCoreAsset : SuCaiFlowEntityFrameworkCoreAsset<string, SuCaiFlowEntityFrameworkCoreTask> {
    public SuCaiFlowEntityFrameworkCoreAsset() {
        Id = Guid.CreateVersion7().ToString();
    }
}

public class SuCaiFlowEntityFrameworkCoreAsset<TKey> : SuCaiFlowEntityFrameworkCoreAsset<TKey, SuCaiFlowEntityFrameworkCoreTask<TKey>>
    where TKey : notnull, IEquatable<TKey> {
}

public class SuCaiFlowEntityFrameworkCoreAsset<TKey, TTask>
    where TKey : notnull, IEquatable<TKey>
    where TTask : class {

    public virtual TKey? Id { get; set; }

    public virtual int OrderNo { get; set; }

    public virtual string? Name { get; set; }

    public virtual string? Title { get; set; }

    public virtual string? Status { get; set; }

    public virtual string? Description { get; set; }

    public virtual string? OriginalUrl { get; set; }

    public virtual string? LandingUrl { get; set; }

    public virtual string? ObjectKey { get; set; }

    public virtual string? ContentType { get; set; }

    public virtual long Size { get; set; }

    public virtual TTask? Task { get; set; }

    public virtual DateTime? CreatedAt { get; set; }

    public virtual DateTime? DownloadedAt { get; set; }
}
