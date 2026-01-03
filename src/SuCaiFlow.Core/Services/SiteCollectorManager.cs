using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SuCaiFlow.Contracts.Services;

namespace SuCaiFlow.Core.Services;

/// <summary>
/// 站点采集器管理器实现
/// </summary>
public class SiteCollectorManager(ILogger<SiteCollectorManager> logger) : ISiteCollectorManager {

    private readonly ILogger<SiteCollectorManager> _logger = logger;
    private readonly List<ISiteCollector> _collectors = [];

    public void RegisterCollector(ISiteCollector collector) {
        if (_collectors.Any(c => c.SiteIdentifier.Equals(collector.SiteIdentifier, StringComparison.OrdinalIgnoreCase))) {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("采集器 {SiteIdentifier} 已经注册", collector.SiteIdentifier);
            return;
        }

        _collectors.Add(collector);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("注册了采集器: {SiteIdentifier} ({DisplayName})", collector.SiteIdentifier, collector.DisplayName);
    }

    public ISiteCollector? GetCollectorForUrl(string url) {
        var collector = _collectors.FirstOrDefault(c => c.CanHandle(url));
        if (collector != null) {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("为URL {Url} 找到采集器: {SiteIdentifier}", url, collector.SiteIdentifier);
        }
        else {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("未找到可以处理URL {Url} 的采集器", url);
        }
        return collector;
    }

    public IEnumerable<ISiteCollector> GetAllCollectors() {
        return _collectors;
    }

    public ISiteCollector? GetCollectorByIdentifier(string siteIdentifier) {
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
