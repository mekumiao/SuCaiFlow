using System.ComponentModel.DataAnnotations;

namespace SuCaiFlow.Contracts.Entities;

public class CollectionTaskConfig {
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Selector { get; set; } = string.Empty;

    public string? SiteUrl { get; set; }

    public string? PaginationSelector { get; set; }

    public int MaxParseItems { get; set; } = 100;

    public int RequestDelayMs { get; set; } = 500;

    public bool EnableAutoPagination { get; set; } = true;

    public string? NextPagePattern { get; set; }

    public Dictionary<string, string> AdditionalSettings { get; set; } = [];

    public Dictionary<string, string> RequestHeaders { get; set; } = [];

    public Dictionary<string, string> SearchParameters { get; set; } = [];

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
