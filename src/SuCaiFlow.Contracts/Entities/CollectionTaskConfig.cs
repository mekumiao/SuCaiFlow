using System.ComponentModel.DataAnnotations;

namespace SuCaiFlow.Contracts.Entities
{
    public class CollectionTaskConfig
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string SiteUrl { get; set; } = string.Empty;
        
        public string Selector { get; set; } = string.Empty;
        
        public string PaginationSelector { get; set; } = string.Empty;
        
        public string NextPagePattern { get; set; } = string.Empty;
        
        public Dictionary<string, string> RequestHeaders { get; set; } = new();
        
        public Dictionary<string, string> SearchParameters { get; set; } = new();
        
        public Dictionary<string, string> AdditionalSettings { get; set; } = new();
        
        public int MaxConcurrency { get; set; } = 5;
        
        public int MaxRetries { get; set; } = 3;
        
        public int MaxParseItems { get; set; } = 100;
        
        public int RequestDelayMs { get; set; } = 500;
        
        public bool EnableAutoPagination { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
