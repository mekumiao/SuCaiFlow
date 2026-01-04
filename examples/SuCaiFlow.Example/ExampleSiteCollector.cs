using SuCaiFlow.Abstractions;
using SuCaiFlow.Engine;

namespace SuCaiFlow.Example;

/// <summary>
/// 示例站点采集器实现
/// </summary>
public class ExampleSiteCollector : ISuCaiFlowEngineSiteCollector {
    public string SiteIdentifier => "example.com";
    public string DisplayName => "示例站点采集器";

    public async Task<IEnumerable<string>> ParsePageAsync(SuCaiFlowTaskDescriptor task, string pageUrl, CancellationToken cancellationToken) {
        // 模拟解析页面，返回一些示例URL
        await Task.Delay(100, cancellationToken); // 模拟网络请求延迟

        var urls = new List<string>();
        for (int i = 0; i < 5; i++) {
            urls.Add($"https://example.com/image{i}.jpg");
        }

        return urls;
    }

    public async Task<SuCaiFlowAssetDescriptor?> DownloadAssetAsync(Uri uri, CancellationToken cancellationToken) {
        // 模拟下载资源
        await Task.Delay(50, cancellationToken); // 模拟下载延迟

        // 创建一个模拟的资源对象
        var asset = new SuCaiFlowAssetDescriptor {
            TaskId = Guid.Empty.ToString(), // 在实际使用中会被设置
            Name = $"Asset_{Guid.NewGuid()}",
            OriginalUri = uri,
            StorageName = $"./downloads/{Guid.NewGuid()}.jpg",
            ContentType = "image/jpeg",
            Status = SuCaiFlowConstants.DownloadStatuses.Completed,
            CreatedAt = DateTime.UtcNow
        };

        return asset;
    }

    public bool CanHandle(string url) {
        throw new NotImplementedException();
    }

    public string ConstructNextPageUrlByPattern(string baseUrl, int pageNumber, string pattern) {
        throw new NotImplementedException();
    }

    public string ConstructNextPageUrl(string baseUrl, int pageNumber) {
        throw new NotImplementedException();
    }

    public string GetDefaultSelector() {
        throw new NotImplementedException();
    }

    public string PreprocessAssetUrl(string url) {
        throw new NotImplementedException();
    }
}
