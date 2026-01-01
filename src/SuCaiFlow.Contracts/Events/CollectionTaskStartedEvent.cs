namespace SuCaiFlow.Contracts.Events;

public class CollectionTaskStartedEvent {
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
