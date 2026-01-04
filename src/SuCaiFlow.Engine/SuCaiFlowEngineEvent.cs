namespace SuCaiFlow.Engine;

public class SuCaiFlowTaskStartedEvent {
    public required string TaskId { get; set; }
    public string? TaskName { get; set; }
    public string? Url { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public class SuCaiFlowTaskProgressEvent {
    public required string TaskId { get; set; }
    public int AssetsCollectedCount { get; set; }
    public int TotalAssetsExpected { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public class SuCaiFlowTaskFailedEvent {
    public required string TaskId { get; set; }
    public string? TaskName { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public class SuCaiFlowTaskCompletedEvent {
    public Guid TaskId { get; set; }
    public string? TaskName { get; set; }
    public int AssetsCollectedCount { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public class AssetDownloadStartedEvent {
    public required string TaskId { get; set; }
    public required string AssetId { get; set; }
    public string? AssetUrl { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public class SuCaiFlowAssetDownloadProgressEvent {
    public required string TaskId { get; set; }
    public required string AssetId { get; set; }
    public string? AssetUrl { get; set; }
    public long BytesDownloaded { get; set; }
    public long? TotalSize { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public class SuCaiFlowAssetDownloadCompletedEvent {
    public required string TaskId { get; set; }
    public required string AssetId { get; set; }
    public string? AssetUrl { get; set; }
    public long? Size { get; set; }
    public string? ContentType { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}
