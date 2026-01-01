using System.ComponentModel.DataAnnotations;

namespace SuCaiFlow.Contracts.Entities
{
    public class CollectionTask
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public CollectionTaskStatus Status { get; set; }
        
        public string Url { get; set; } = string.Empty;
        
        public string Selector { get; set; } = string.Empty;
        
        public int MaxConcurrency { get; set; } = 5;
        
        public int MaxRetries { get; set; } = 3;
        
        public Dictionary<string, string> Parameters { get; set; } = new();
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? StartedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public List<CollectedAsset> Assets { get; set; } = new();
        
        public int TotalAssetsExpected { get; set; } = 0;
        
        public int AssetsCollectedCount { get; set; } = 0;
        
        public Guid? ConfigId { get; set; }
        
        public CollectionTaskConfig? Config { get; set; }
    }
}
