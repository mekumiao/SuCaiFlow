namespace SuCaiFlow.Contracts.Events;

public class CollectionTaskStartedEvent {
    public Guid TaskId { get; set; }
    public string? TaskName { get; set; }
    public string? Url { get; set; }
    public DateTime? Timestamp { get; set; }
}
