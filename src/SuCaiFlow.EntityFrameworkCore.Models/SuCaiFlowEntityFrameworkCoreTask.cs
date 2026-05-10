using System.Diagnostics.CodeAnalysis;

namespace SuCaiFlow.EntityFrameworkCore.Models;

public class SuCaiFlowEntityFrameworkCoreTask : SuCaiFlowEntityFrameworkCoreTask<string, SuCaiFlowEntityFrameworkCoreAsset> {
    public SuCaiFlowEntityFrameworkCoreTask() {
        Id = Guid.CreateVersion7().ToString();
    }
}

public class SuCaiFlowEntityFrameworkCoreTask<TKey> : SuCaiFlowEntityFrameworkCoreTask<TKey, SuCaiFlowEntityFrameworkCoreAsset<TKey>>
    where TKey : notnull, IEquatable<TKey> {
}

public class SuCaiFlowEntityFrameworkCoreTask<TKey, TAsset>
    where TKey : notnull, IEquatable<TKey>
    where TAsset : class {

    public virtual TKey? Id { get; set; }

    public virtual string? Name { get; set; }

    public virtual string? DisplayName { get; set; }

    public virtual string? Description { get; set; }

    public virtual string? SiteIdentifier { get; set; }

    public virtual string? SearchKeywords { get; set; }

    /// <summary>
    /// 开始的URL,不限长度
    /// </summary>
    public virtual string? StartUrl { get; set; }

    public virtual string? Status { get; set; }

    public virtual DateTime? CreatedAt { get; set; }

    public virtual DateTime? StartedAt { get; set; }

    public virtual DateTime? CompletedAt { get; set; }

    public virtual int AssetsDownloadCount { get; set; }

    public virtual int AssetsCollectedCount { get; set; }

    public virtual int AssetsToCollectCount { get; set; }

    public virtual string? ErrorMessage { get; set; }

    [StringSyntax(StringSyntaxAttribute.Json)]
    public virtual string? Parameters { get; set; }

    public virtual ICollection<TAsset> Assets { get; } = new HashSet<TAsset>();

    public virtual bool KeepAfterCancel { get; set; }
}
