namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineOptions {
    public int MaxConcurrency { get; set; } = 5;
    public int QueueCapacity { get; set; } = 50;
    public int RequestDelayMs { get; set; } = 1000;
    public int MaxDownloadConcurrency { get; set; } = 40;
    public int DownloadQueueCapacity { get; set; } = 500;
    public HashSet<Type> SiteCollectorImplementTypes { get; } = [];
    public TimeProvider TimeProvider { get; set; } = default!;
}
