using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowDownloadTaskContext(
    SuCaiFlowTaskDescriptor descriptor,
    SuCaiFlowAssetDescriptor assetDescriptor,
    ISuCaiFlowEngineSiteCollector collector,
    CancellationToken ct) {
    public SuCaiFlowTaskDescriptor Descriptor { get; } = descriptor;
    public SuCaiFlowAssetDescriptor AssetDescriptor { get; } = assetDescriptor;
    public ISuCaiFlowEngineSiteCollector Collector { get; } = collector;
    public CancellationToken CancellationToken { get; } = ct;
    public TaskCompletionSource Completion { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
}
