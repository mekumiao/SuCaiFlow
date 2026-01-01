namespace SuCaiFlow.Contracts.Events;

public class AssetDownloadStartedEvent {
    public Guid TaskId { get; set; }
    public Guid AssetId { get; set; }
    public string AssetUrl { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
