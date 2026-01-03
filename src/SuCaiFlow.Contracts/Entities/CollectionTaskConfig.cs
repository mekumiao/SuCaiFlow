namespace SuCaiFlow.Contracts.Entities;

public class CollectionTaskConfig {
    public virtual Guid Id { get; set; }

    public virtual string? Name { get; set; }

    public virtual string? Description { get; set; }

    public virtual string? Selector { get; set; }

    public virtual string? SiteUrl { get; set; }

    public virtual string? PaginationSelector { get; set; }

    public virtual int MaxParseItems { get; set; }

    public virtual int RequestDelayMs { get; set; }

    public virtual bool EnableAutoPagination { get; set; }

    public virtual string? NextPagePattern { get; set; }

    public virtual Dictionary<string, string>? AdditionalSettings { get; set; }

    public virtual Dictionary<string, string>? RequestHeaders { get; set; }

    public virtual Dictionary<string, string>? SearchParameters { get; set; }

    public virtual DateTime? CreatedAt { get; set; }

    public virtual DateTime? UpdatedAt { get; set; }
}

public enum CollectionTaskStatus {
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
