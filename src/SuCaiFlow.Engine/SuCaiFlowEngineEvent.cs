namespace SuCaiFlow.Engine;

public record SuCaiFlowTaskStartedEvent {
    public required string TaskId { get; set; }
    public int AssetsToCollectCount { get; set; }
}

public record SuCaiFlowTaskProgressEvent {
    public required string TaskId { get; set; }
    public int AssetsCollectedCount { get; set; }
    public int AssetsToCollectCount { get; set; }
}

public record SuCaiFlowTaskCanceledEvent {
    public required string TaskId { get; set; }
    public int AssetsCollectedCount { get; set; }
    public int AssetsToCollectCount { get; set; }
}

public record SuCaiFlowTaskFailedEvent {
    public required string TaskId { get; set; }
    public string? ErrorMessage { get; set; }
}

public record SuCaiFlowTaskCompletedEvent {
    public required string TaskId { get; set; }
    public int AssetsCollectedCount { get; set; }
    public int AssetsToCollectCount { get; set; }
}

public record SuCaiFlowTaskDownloadProgressEvent {
    public required string TaskId { get; set; }
    public int AssetsDownloadCount { get; set; }
    public int AssetsToCollectCount { get; set; }
}
