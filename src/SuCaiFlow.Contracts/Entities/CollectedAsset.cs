using System.ComponentModel.DataAnnotations;

namespace SuCaiFlow.Contracts.Entities
{
    public class CollectedAsset
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string Url { get; set; } = string.Empty;
        
        public string LocalPath { get; set; } = string.Empty;
        
        public string ContentType { get; set; } = string.Empty;
        
        public long Size { get; set; }
        
        public Guid CollectionTaskId { get; set; }
        
        public CollectionTask? CollectionTask { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? DownloadedAt { get; set; }
        
        public AssetStatus Status { get; set; } = AssetStatus.Pending;
        
        public string? ErrorMessage { get; set; }
    }
}
