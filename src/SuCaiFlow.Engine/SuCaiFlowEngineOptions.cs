namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineOptions {
    public int MaxConcurrentTasks { get; set; } = 10;
    public int MaxConcurrentDownloads { get; set; } = 20;
    public int ChannelBufferFactor { get; set; } = 50;
    public HashSet<Type> SiteCollectorImplementTypes { get; } = [];
    public TimeProvider TimeProvider { get; set; } = default!;
}
