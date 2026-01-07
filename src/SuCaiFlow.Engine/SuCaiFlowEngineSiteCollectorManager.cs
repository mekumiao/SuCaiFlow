using Microsoft.Extensions.Logging;

namespace SuCaiFlow.Engine;

/// <summary>
/// 站点采集器管理器实现
/// </summary>
public class SuCaiFlowEngineSiteCollectorManager(ILogger<SuCaiFlowEngineSiteCollectorManager> logger) : ISuCaiFlowEngineSiteCollectorManager {

    private readonly ILogger<SuCaiFlowEngineSiteCollectorManager> _logger = logger;
    private readonly List<ISuCaiFlowEngineSiteCollector> _collectors = [];

    public void RegisterCollector(ISuCaiFlowEngineSiteCollector collector) {
        if (_collectors.Any(c => c.SiteIdentifier.Equals(collector.SiteIdentifier, StringComparison.OrdinalIgnoreCase))) {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("采集器 {SiteIdentifier} 已经注册", collector.SiteIdentifier);
            return;
        }

        _collectors.Add(collector);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("注册了采集器: {SiteIdentifier}", collector.SiteIdentifier);
    }

    public ISuCaiFlowEngineSiteCollector? GetCollectorForUrl(string url) {
        var collector = _collectors.FirstOrDefault(c => c.CanHandle(url));
        return collector;
    }

    public ISuCaiFlowEngineSiteCollector? GetCollectorByIdentifier(string siteIdentifier) {
        var collector = _collectors.FirstOrDefault(c =>
            c.SiteIdentifier.Equals(siteIdentifier, StringComparison.OrdinalIgnoreCase));
        return collector;
    }
}
