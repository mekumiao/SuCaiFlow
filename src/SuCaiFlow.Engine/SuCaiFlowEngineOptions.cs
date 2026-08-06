namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineOptions {
    public const string SectionName = "SuCaiFlowEngine";

    public int MaxConcurrency { get; set; } = 5;
    public int QueueCapacity { get; set; } = 50;
    public int RequestDelayMs { get; set; } = 1000;
    public int MaxDownloadConcurrency { get; set; } = 20;
    public int DownloadQueueCapacity { get; set; } = 200;
    public TimeProvider TimeProvider { get; set; } = default!;
}
