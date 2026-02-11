using System.Diagnostics.CodeAnalysis;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public interface ISuCaiFlowEngineSiteCollector {

    string SiteIdentifier { get; }

    bool CanHandle([NotNullWhen(true)] string? url);

    string? ParseObjectKey(SuCaiFlowAssetDescriptor assetDescriptor);

    Task<List<SuCaiFlowAssetDescriptor>> ParsePageAsync(SuCaiFlowTaskDescriptor descriptor, int pageNum, CancellationToken ct);

    Task DownloadAssetAsync(SuCaiFlowAssetDescriptor descriptor, CancellationToken ct);
}
