namespace SuCaiFlow.Contracts.Events;

public class CollectionTaskFailedEvent {
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
