using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Services;

/// <summary>
/// 站点采集器接口，定义了采集特定站点资源的方法
/// </summary>
public interface ISiteCollector {
    /// <summary>
    /// 获取站点采集器的唯一标识符
    /// </summary>
    string SiteIdentifier { get; }

    /// <summary>
    /// 获取站点采集器的显示名称
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 检查此采集器是否可以处理指定的URL
    /// </summary>
    /// <param name="url">要检查的URL</param>
    /// <returns>如果此采集器可以处理该URL则返回true，否则返回false</returns>
    bool CanHandle(string url);

    /// <summary>
    /// 异步解析页面获取资源URL列表
    /// </summary>
    /// <param name="task">采集任务</param>
    /// <param name="pageUrl">页面URL</param>
    /// <param name="config">任务配置</param>
    /// <param name="cancellationToken"></param>
    /// <returns>资源URL列表</returns>
    Task<IEnumerable<string>> ParsePageAsync(CollectionTask task, string pageUrl, CollectionTaskConfig? config, CancellationToken cancellationToken);

    /// <summary>
    /// 根据配置模式构造下一页URL
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="pattern">翻页模式</param>
    /// <returns>下一页URL</returns>
    string ConstructNextPageUrlByPattern(string baseUrl, int pageNumber, string pattern);

    /// <summary>
    /// 构造下一页URL（默认模式）
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="pageNumber">页码</param>
    /// <returns>下一页URL</returns>
    string ConstructNextPageUrl(string baseUrl, int pageNumber);

    /// <summary>
    /// 获取站点特定的默认选择器
    /// </summary>
    /// <returns>默认选择器字符串</returns>
    string GetDefaultSelector();

    /// <summary>
    /// 异步下载资源
    /// </summary>
    /// <param name="url">资源URL</param>
    /// <param name="config">任务配置</param>
    /// <param name="cancellationToken"></param>
    /// <returns>下载的资源对象，如果下载失败则返回null</returns>
    Task<CollectedAsset?> DownloadAssetAsync(string url, CollectionTaskConfig? config, CancellationToken cancellationToken);

    /// <summary>
    /// 预处理资源URL，例如处理防盗链、获取真实下载链接等
    /// </summary>
    /// <param name="url">原始资源URL</param>
    /// <param name="config">任务配置</param>
    /// <returns>处理后的资源URL</returns>
    string PreprocessAssetUrl(string url, CollectionTaskConfig? config = null);
}
