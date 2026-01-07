using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public interface ISuCaiFlowEngineSiteCollector {

    string SiteIdentifier { get; }

    bool CanHandle(string url);

    Task<IEnumerable<SuCaiFlowAssetDescriptor>> ParsePageAsync(SuCaiFlowTaskDescriptor descriptor, int pageNum, CancellationToken cancellationToken);

    Task DownloadAssetAsync(SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken);
}
