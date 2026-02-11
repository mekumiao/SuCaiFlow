using SuCaiFlow.Abstractions;
using SuCaiFlow.Engine;

namespace SuCaiFlow.Example;

public class ExampleSiteCollector : ISuCaiFlowEngineSiteCollector {
    public string SiteIdentifier => "example.com";

    public async Task<List<SuCaiFlowAssetDescriptor>> ParsePageAsync(SuCaiFlowTaskDescriptor descriptor, int pageNum, CancellationToken ct) {
        await Task.Delay(100, ct);

        var urls = new List<string>();
        for (int i = 0; i < 5; i++) {
            urls.Add($"https://example.com/image{i}.jpg");
        }
        var asset = new SuCaiFlowAssetDescriptor {
            TaskId = Guid.Empty.ToString(),
            Name = $"Asset_{Guid.NewGuid()}",
            OriginalUrl = "https://www.baidu.com/xxx.jpg",
            ObjectKey = $"./downloads/{Guid.NewGuid()}.jpg",
            ContentType = "image/jpeg",
            Status = SuCaiFlowConstants.DownloadStatuses.Completed,
            CreatedAt = DateTime.UtcNow
        };
        return [asset];
    }

    public async Task DownloadAssetAsync(SuCaiFlowAssetDescriptor descriptor, CancellationToken ct) {
        await Task.Delay(50, ct);
    }

    public bool CanHandle(string? url) {
        throw new NotImplementedException();
    }

    public string? ParseObjectKey(SuCaiFlowAssetDescriptor assetDescriptor) {
        throw new NotImplementedException();
    }
}
