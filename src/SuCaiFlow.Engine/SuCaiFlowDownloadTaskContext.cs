using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowDownloadTaskContext(
    string taskId,
    SuCaiFlowTaskDescriptor descriptor,
    SuCaiFlowAssetDescriptor assetDescriptor,
    ISuCaiFlowEngineSiteCollector collector,
    CancellationToken ct) {
    public string TaskId { get; } = taskId;
    public SuCaiFlowTaskDescriptor Descriptor { get; } = descriptor;
    public SuCaiFlowAssetDescriptor AssetDescriptor { get; } = assetDescriptor;
    public ISuCaiFlowEngineSiteCollector Collector { get; } = collector;
    public CancellationToken CancellationToken { get; } = ct;
    public TaskCompletionSource Completion { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
}
