using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public interface ISuCaiFlowEngineSiteCollector {

    string SiteIdentifier { get; }

    bool CanHandle(string url);

    string? ParseObjectKey(SuCaiFlowAssetDescriptor assetDescriptor);

    Task<List<SuCaiFlowAssetDescriptor>> ParsePageAsync(SuCaiFlowTaskDescriptor descriptor, int pageNum, CancellationToken cancellationToken);

    Task DownloadAssetAsync(SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken);
}
