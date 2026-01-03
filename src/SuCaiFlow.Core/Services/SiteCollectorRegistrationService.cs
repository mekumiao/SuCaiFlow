using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SuCaiFlow.Contracts.Services;

namespace SuCaiFlow.Core.Services;

/// <summary>
/// 站点采集器注册服务，用于自动注册所有实现ISiteCollector接口的服务
/// </summary>
public class SiteCollectorRegistrationService(
    IServiceProvider serviceProvider,
    ISiteCollectorManager collectorManager,
    ILogger<SiteCollectorRegistrationService> logger) {
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ISiteCollectorManager _collectorManager = collectorManager;
    private readonly ILogger<SiteCollectorRegistrationService> _logger = logger;

    /// <summary>
    /// 自动注册所有站点采集器
    /// </summary>
    public void RegisterAllCollectors() {
        // 从服务提供者获取所有ISiteCollector服务
        using var scope = _serviceProvider.CreateScope();
        var serviceDescriptors = _serviceProvider
            .GetService<IServiceCollection>()?.Where(d =>
                d.ServiceType == typeof(ISiteCollector) &&
                d.ImplementationType != null)
            .ToList() ?? [];

        foreach (var descriptor in serviceDescriptors) {
            if (descriptor.ImplementationType != null) {
                try {
                    if (scope.ServiceProvider.GetService(descriptor.ImplementationType) is ISiteCollector collector) {
                        _collectorManager.RegisterCollector(collector);
                    }
                }
                catch (Exception ex) {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogError(ex, "注册站点采集器 {ServiceType} 时出错", descriptor.ImplementationType?.Name);
                }
            }
        }

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("自动注册了 {Count} 个站点采集器", _collectorManager.GetAllCollectors().Count());
    }
}
