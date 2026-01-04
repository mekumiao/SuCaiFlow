using System.Diagnostics.CodeAnalysis;

namespace SuCaiFlow.EntityFrameworkCore.Models;

public class SuCaiFlowEntityFrameworkCoreTask : SuCaiFlowEntityFrameworkCoreTask<string, SuCaiFlowEntityFrameworkCoreAsset> {
    public SuCaiFlowEntityFrameworkCoreTask() {
        Id = Guid.NewGuid().ToString();
    }
}

public class SuCaiFlowEntityFrameworkCoreTask<TKey> : SuCaiFlowEntityFrameworkCoreTask<TKey, SuCaiFlowEntityFrameworkCoreAsset<TKey>>
    where TKey : notnull, IEquatable<TKey> {
}

public class SuCaiFlowEntityFrameworkCoreTask<TKey, TAsset>
    where TKey : notnull, IEquatable<TKey>
    where TAsset : class {

    public virtual required TKey Id { get; set; }

    public virtual string? DisplayName { get; set; }

    public virtual string? Description { get; set; }

    public virtual string? StartUri { get; set; }

    public virtual string? Status { get; set; }

    public virtual DateTime? CreatedAt { get; set; }

    public virtual DateTime? StartedAt { get; set; }

    public virtual DateTime? CompletedAt { get; set; }

    public virtual int AssetsDownloadCount { get; set; }

    public virtual int TotalAssetsExpected { get; set; }

    public virtual string? ErrorMessage { get; set; }

    [StringSyntax(StringSyntaxAttribute.Json)]
    public virtual string? Parameters { get; set; }

    public virtual ICollection<TAsset> Assets { get; } = new HashSet<TAsset>();

    public virtual string? ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();
}
