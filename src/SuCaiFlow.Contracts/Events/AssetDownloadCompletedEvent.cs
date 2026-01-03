namespace SuCaiFlow.Contracts.Events;

public class AssetDownloadCompletedEvent {
    public Guid TaskId { get; set; }
    public Guid AssetId { get; set; }
    public string? AssetUrl { get; set; }
    public long? Size { get; set; }
    public string? ContentType { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? Timestamp { get; set; }
}
