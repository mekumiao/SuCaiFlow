namespace SuCaiFlow.Core;

public class SuCaiFlowCoreOptions {
    public int MaxConcurrency { get; set; } = 5;
    public TimeProvider TimeProvider { get; set; } = default!;
    public HashSet<Type> SiteCollectorImplementTypes { get; } = [];
}
