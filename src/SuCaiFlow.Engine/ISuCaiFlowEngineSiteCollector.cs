using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public interface ISuCaiFlowEngineSiteCollector {

    string SiteIdentifier { get; }

    bool CanHandle(Uri uri);

    Task<IEnumerable<SuCaiFlowAssetDescriptor>> ParsePageAsync(SuCaiFlowTaskDescriptor descriptor, int pageNum, CancellationToken cancellationToken);

    Task<SuCaiFlowAssetDescriptor?> DownloadAssetAsync(Uri uri, CancellationToken cancellationToken);
}
