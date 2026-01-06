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

    public ISuCaiFlowEngineSiteCollector? GetCollectorForUrl(Uri uri) {
        var collector = _collectors.FirstOrDefault(c => c.CanHandle(uri));
        if (collector != null) {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("为URL {Url} 找到采集器: {SiteIdentifier}", uri, collector.SiteIdentifier);
        }
        else {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("未找到可以处理URL {Url} 的采集器", uri);
        }
        return collector;
    }

    public IEnumerable<ISuCaiFlowEngineSiteCollector> GetAllCollectors() {
        return _collectors;
    }

    public ISuCaiFlowEngineSiteCollector? GetCollectorByIdentifier(string siteIdentifier) {
        var collector = _collectors.FirstOrDefault(c =>
            c.SiteIdentifier.Equals(siteIdentifier, StringComparison.OrdinalIgnoreCase));

        if (collector != null) {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("找到采集器: {SiteIdentifier}", siteIdentifier);
        }
        else {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("未找到标识符为 {SiteIdentifier} 的采集器", siteIdentifier);
        }

        return collector;
    }
}
