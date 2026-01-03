namespace SuCaiFlow.Contracts.Events;

public class AssetDownloadProgressEvent {
    public Guid TaskId { get; set; }
    public Guid AssetId { get; set; }
    public string? AssetUrl { get; set; }
    public long BytesDownloaded { get; set; }
    public long? TotalSize { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime? Timestamp { get; set; }
}
