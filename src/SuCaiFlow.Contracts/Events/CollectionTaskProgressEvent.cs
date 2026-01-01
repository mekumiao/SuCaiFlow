namespace SuCaiFlow.Contracts.Events;

public class CollectionTaskProgressEvent {
    public Guid TaskId { get; set; }
    public int AssetsCollectedCount { get; set; }
    public int TotalAssetsExpected { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
