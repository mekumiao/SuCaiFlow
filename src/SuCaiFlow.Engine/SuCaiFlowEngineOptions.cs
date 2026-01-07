namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineOptions {
    public int ChannelConcurrency { get; set; } = 5;
    public int ChannelCapacity { get; set; } = 2000;
    public HashSet<Type> SiteCollectorImplementTypes { get; } = [];
    public TimeProvider TimeProvider { get; set; } = default!;
}
