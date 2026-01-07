namespace SuCaiFlow.Engine;

public record SuCaiFlowTaskStartedEvent {
    public required string TaskId { get; set; }
}

public record SuCaiFlowTaskProgressEvent {
    public required string TaskId { get; set; }
    public int AssetsCollectedCount { get; set; }
    public int TotalAssetsExpected { get; set; }
}

public record SuCaiFlowTaskCanceledEvent {
    public required string TaskId { get; set; }
    public int AssetsCollectedCount { get; set; }
    public int TotalAssetsExpected { get; set; }
}

public record SuCaiFlowTaskFailedEvent {
    public required string TaskId { get; set; }
    public string? ErrorMessage { get; set; }
}

public record SuCaiFlowTaskCompletedEvent {
    public required string TaskId { get; set; }
    public int AssetsCollectedCount { get; set; }
    public int TotalAssetsExpected { get; set; }
    public string? Status { get; set; }
}

public record AssetDownloadStartedEvent {
    public required string TaskId { get; set; }
    public required string AssetId { get; set; }
}

public record SuCaiFlowAssetDownloadProgressEvent {
    public required string TaskId { get; set; }
    public required string AssetId { get; set; }
    public long BytesDownloaded { get; set; }
    public long? TotalSize { get; set; }
}

public class SuCaiFlowAssetDownloadCompletedEvent {
    public required string TaskId { get; set; }
    public required string AssetId { get; set; }
    public string? Status { get; set; }
    public long Size { get; set; }
}
