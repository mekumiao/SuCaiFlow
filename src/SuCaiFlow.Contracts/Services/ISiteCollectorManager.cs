namespace SuCaiFlow.Contracts.Services;

/// <summary>
/// 站点采集器管理器接口，负责管理和调度不同的站点采集器
/// </summary>
public interface ISiteCollectorManager {
    /// <summary>
    /// 注册站点采集器
    /// </summary>
    /// <param name="collector">站点采集器实例</param>
    void RegisterCollector(ISiteCollector collector);

    /// <summary>
    /// 获取可以处理指定URL的站点采集器
    /// </summary>
    /// <param name="url">要处理的URL</param>
    /// <returns>站点采集器实例，如果找不到则返回null</returns>
    ISiteCollector? GetCollectorForUrl(string url);

    /// <summary>
    /// 获取所有注册的站点采集器
    /// </summary>
    /// <returns>所有站点采集器列表</returns>
    IEnumerable<ISiteCollector> GetAllCollectors();

    /// <summary>
    /// 根据站点标识符获取采集器
    /// </summary>
    /// <param name="siteIdentifier">站点标识符</param>
    /// <returns>站点采集器实例，如果找不到则返回null</returns>
    ISiteCollector? GetCollectorByIdentifier(string siteIdentifier);
}
