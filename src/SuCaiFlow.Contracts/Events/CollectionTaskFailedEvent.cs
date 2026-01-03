namespace SuCaiFlow.Contracts.Events;

public class CollectionTaskFailedEvent {
    public Guid TaskId { get; set; }
    public string? TaskName { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? Timestamp { get; set; }
}
