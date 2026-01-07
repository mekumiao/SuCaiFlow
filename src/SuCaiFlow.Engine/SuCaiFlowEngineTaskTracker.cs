using System.Collections.Concurrent;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineTaskTracker {
    private readonly ConcurrentDictionary<string, Lazy<Task<SuCaiFlowTaskDescriptor>>> _tasks = new();

    public async Task<(bool Created, SuCaiFlowTaskDescriptor? Task)> TryCreateAsync(
        string taskId,
        Func<string, Task<SuCaiFlowTaskDescriptor>> descriptorFactory) {
        var lazy = new Lazy<Task<SuCaiFlowTaskDescriptor>>(
            () => descriptorFactory(taskId),
            LazyThreadSafetyMode.ExecutionAndPublication);

        var existing = _tasks.GetOrAdd(taskId, lazy);

        if (!ReferenceEquals(existing, lazy)) {
            return (false, null);
        }

        try {
            var descriptor = await existing.Value;
            return (true, descriptor);
        }
        catch {
            _tasks.TryRemove(taskId, out _);
            throw;
        }
    }

    public void MarkRunning(string taskId) {
        if (_tasks.TryGetValue(taskId, out var lazy) && lazy.IsValueCreated) {
            lazy.Value.Result.Status = SuCaiFlowConstants.TaskStatuses.Running;
            lazy.Value.Result.StartedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkCompleted(string taskId) {
        if (_tasks.TryRemove(taskId, out var lazy) && lazy.IsValueCreated) {
            var task = lazy.Value.Result;
            task.Status = SuCaiFlowConstants.TaskStatuses.Completed;
            task.CompletedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkCanceled(string taskId) {
        if (_tasks.TryRemove(taskId, out var lazy) && lazy.IsValueCreated) {
            var task = lazy.Value.Result;
            task.Status = SuCaiFlowConstants.TaskStatuses.Canceled;
            task.CompletedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkFailed(string taskId, string? message) {
        if (_tasks.TryRemove(taskId, out var lazy) && lazy.IsValueCreated) {
            var task = lazy.Value.Result;
            task.Status = SuCaiFlowConstants.TaskStatuses.Failed;
            task.ErrorMessage = message;
            task.CompletedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool TryGet(string taskId, out SuCaiFlowTaskDescriptor? task) {
        task = null;
        if (_tasks.TryGetValue(taskId, out var lazy) && lazy.IsValueCreated) {
            task = lazy.Value.Result;
            return true;
        }
        return false;
    }

    public IReadOnlyCollection<SuCaiFlowTaskDescriptor> GetActiveTasks() {
        return [.. _tasks.Values
            .Where(lazy => lazy.IsValueCreated)
            .Select(lazy => lazy.Value.Result)];
    }
}
