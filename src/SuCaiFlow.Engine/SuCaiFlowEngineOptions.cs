namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineOptions {
    public int MaxConcurrency { get; set; } = 5;
    public HashSet<Type> SiteCollectorImplementTypes { get; } = [];
    public TimeProvider TimeProvider { get; set; } = default!;
}
