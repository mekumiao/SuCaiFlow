using System.Text.Json;

namespace SuCaiFlow.Abstractions;

public class SuCaiFlowTaskDescriptor {
    public string? TaskId { get; set; }

    public string? DisplayName { get; set; }

    public Uri? StartUri { get; set; }

    public string? Status { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public int AssetsCollectedCount { get; set; }

    public int TotalAssetsExpected { get; set; }

    public Dictionary<string, JsonElement> Parameters { get; } = new(StringComparer.Ordinal);
}
