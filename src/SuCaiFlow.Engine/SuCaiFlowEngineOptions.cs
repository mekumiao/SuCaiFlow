namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineOptions {
    public int MaxConcurrency { get; set; } = 10;
    public int MaxDownloadConcurrency { get; set; } = 20;
    public int QueueCapacity { get; set; } = 500;
    public HashSet<Type> SiteCollectorImplementTypes { get; } = [];
    public TimeProvider TimeProvider { get; set; } = default!;
}
