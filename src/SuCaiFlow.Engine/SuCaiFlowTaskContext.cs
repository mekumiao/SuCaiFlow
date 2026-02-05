using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowTaskContext(
    string taskId,
    SuCaiFlowTaskDescriptor descriptor) {
    public string TaskId { get; } = taskId;
    public SuCaiFlowTaskDescriptor Descriptor { get; } = descriptor;
    public CancellationTokenSource Cancellation { get; } = new CancellationTokenSource();
    public TaskCompletionSource Completion { get; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
}
