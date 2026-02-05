using Microsoft.Extensions.Logging;

namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineSiteCollectorManager(ILogger<SuCaiFlowEngineSiteCollectorManager> logger)
    : ISuCaiFlowEngineSiteCollectorManager {

    private readonly ILogger<SuCaiFlowEngineSiteCollectorManager> _logger = logger;
    private readonly Dictionary<string, ISuCaiFlowEngineSiteCollector> _collectors = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterCollector(ISuCaiFlowEngineSiteCollector collector) {
        if (_collectors.TryAdd(collector.SiteIdentifier, collector)) {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("注册了采集器: {SiteIdentifier}", collector.SiteIdentifier);
        }
    }

    public ISuCaiFlowEngineSiteCollector? GetCollectorForUrl(string url) {
        return _collectors.Values.FirstOrDefault(c => c.CanHandle(url));
    }

    public ISuCaiFlowEngineSiteCollector? GetCollectorByIdentifier(string siteIdentifier) {
        _collectors.TryGetValue(siteIdentifier, out var collector);
        return collector;
    }
}
