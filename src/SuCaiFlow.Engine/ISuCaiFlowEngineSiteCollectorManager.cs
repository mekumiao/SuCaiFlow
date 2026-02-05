namespace SuCaiFlow.Engine;

public interface ISuCaiFlowEngineSiteCollectorManager {
    void RegisterCollector(ISuCaiFlowEngineSiteCollector collector);
    ISuCaiFlowEngineSiteCollector? GetCollectorForUrl(string url);
    ISuCaiFlowEngineSiteCollector? GetCollectorByIdentifier(string siteIdentifier);
}
