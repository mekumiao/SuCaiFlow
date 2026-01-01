using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Interfaces;
using SuCaiFlow.Core.Services;

namespace SuCaiFlow.Example
{
    /// <summary>
    /// 示例站点采集器实现
    /// </summary>
    public class ExampleSiteCollector : BaseSiteCollector
    {
        public override string SiteIdentifier => "example.com";
        public override string DisplayName => "示例站点采集器";

        public override async Task<List<string>> ParsePageAsync(CollectionTask task, string pageUrl, CollectionTaskConfig? config = null)
        {
            // 模拟解析页面，返回一些示例URL
            await Task.Delay(100); // 模拟网络请求延迟
            
            var urls = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                urls.Add($"https://example.com/image{i}.jpg");
            }
            
            return urls;
        }

        public override async Task<CollectedAsset?> DownloadAssetAsync(string url, CollectionTaskConfig? config = null)
        {
            // 模拟下载资源
            await Task.Delay(50); // 模拟下载延迟
            
            // 创建一个模拟的资源对象
            var asset = new CollectedAsset
            {
                Id = Guid.NewGuid(),
                CollectionTaskId = Guid.Empty, // 在实际使用中会被设置
                Name = $"Asset_{Guid.NewGuid()}",
                Url = url,
                LocalPath = $"./downloads/{Guid.NewGuid()}.jpg",
                ContentType = "image/jpeg",
                Status = AssetStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };

            return asset;
        }
    }
}
