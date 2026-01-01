using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Events;

public class CollectionTaskCompletedEvent {
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public int AssetsCollectedCount { get; set; }
    public CollectionTaskStatus Status { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
