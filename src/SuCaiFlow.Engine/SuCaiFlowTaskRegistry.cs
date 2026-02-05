using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public sealed class SuCaiFlowTaskRegistry {
    private readonly ConcurrentDictionary<string, SuCaiFlowTaskContext> _tasks = new();

    public bool TryRegister(SuCaiFlowTaskDescriptor descriptor, [MaybeNullWhen(false)] out SuCaiFlowTaskContext ctx) {
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.TaskId);

        ctx = null;
        var context = new SuCaiFlowTaskContext(descriptor.TaskId, descriptor);
        if (!_tasks.TryAdd(descriptor.TaskId, context))
            return false;

        ctx = context;
        return true;
    }

    public bool TryGet(string taskId, [MaybeNullWhen(false)] out SuCaiFlowTaskContext ctx) {
        return _tasks.TryGetValue(taskId, out ctx);
    }

    public void Complete(string taskId) {
        if (_tasks.TryRemove(taskId, out var ctx))
            ctx.Cancellation.Dispose();
    }

    public void Cancel(string taskId) {
        if (_tasks.TryGetValue(taskId, out var ctx))
            ctx.Cancellation.Cancel();
    }
}
