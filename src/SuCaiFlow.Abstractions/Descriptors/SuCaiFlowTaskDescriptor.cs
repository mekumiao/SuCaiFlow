using System.Text.Json;

using SuCaiFlow.Abstractions.Descriptors;

namespace SuCaiFlow.Abstractions;

public class SuCaiFlowTaskDescriptor {
    public string? TaskId { get; set; }

    public string? Name { get; set; }

    public string? SearchKeywords { get; set; }

    public string? StartUrl { get; set; }

    public string? SiteIdentifier { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    /// <remarks>
    /// <see cref="SuCaiFlowConstants.TaskStatuses"/>
    /// </remarks>
    public string? Status { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    private int _assetsDownloadCount;
    public int AssetsDownloadCount {
        get => _assetsDownloadCount;
        set => _assetsDownloadCount = value;
    }

    public int AssetsCollectedCount { get; set; }

    public int AssetsToCollectCount { get; set; }

    public Dictionary<string, JsonElement> Parameters { get; } = new(StringComparer.Ordinal);

    public void MarkRunning() {
        Status = SuCaiFlowConstants.TaskStatuses.Running;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void MarkCompleted() {
        Status = SuCaiFlowConstants.TaskStatuses.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkCanceled() {
        Status = SuCaiFlowConstants.TaskStatuses.Canceled;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string? message) {
        Status = SuCaiFlowConstants.TaskStatuses.Failed;
        ErrorMessage = message;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void DownloadIncrement() {
        Interlocked.Increment(ref _assetsDownloadCount);
    }

    public void MapFrom(SuCaiFlowTaskRequest request) {
        Name = request.Name;
        SiteIdentifier = request.SiteIdentifier;
        SearchKeywords = request.SearchKeywords;
        AssetsToCollectCount = request.AssetsToCollectCount;
        StartUrl = request.StartUrl;
        if (request.Parameters != null)
            foreach (var (key, value) in request.Parameters) {
                Parameters[key] = value;
            }
    }
}
