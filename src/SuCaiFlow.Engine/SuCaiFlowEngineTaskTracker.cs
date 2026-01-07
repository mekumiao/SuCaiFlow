using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineTaskTracker {
    private readonly ConcurrentDictionary<string, SuCaiFlowTaskDescriptor> _descriptors = new();

    public bool TryAdd(SuCaiFlowTaskDescriptor descriptor) {
        ArgumentException.ThrowIfNullOrWhiteSpace(descriptor.TaskId);

        return _descriptors.TryAdd(descriptor.TaskId, descriptor);
    }

    public void MarkRunning(string taskId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);

        if (_descriptors.TryGetValue(taskId, out var descriptor)) {
            descriptor.Status = SuCaiFlowConstants.TaskStatuses.Running;
            descriptor.StartedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkCompleted(string taskId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);

        if (_descriptors.TryRemove(taskId, out var descriptor)) {
            descriptor.Status = SuCaiFlowConstants.TaskStatuses.Completed;
            descriptor.CompletedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkCanceled(string taskId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);

        if (_descriptors.TryRemove(taskId, out var descriptor)) {
            descriptor.Status = SuCaiFlowConstants.TaskStatuses.Canceled;
            descriptor.CompletedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkFailed(string taskId, string? message) {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);

        if (_descriptors.TryRemove(taskId, out var descriptor)) {
            descriptor.Status = SuCaiFlowConstants.TaskStatuses.Failed;
            descriptor.ErrorMessage = message;
            descriptor.CompletedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool TryGet(string taskId, [NotNullWhen(true)] out SuCaiFlowTaskDescriptor? descriptor) {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);

        if (_descriptors.TryGetValue(taskId, out descriptor)) {
            return true;
        }
        return false;
    }

    public IReadOnlyCollection<SuCaiFlowTaskDescriptor> GetActiveTasks() {
        return [.. _descriptors.Values];
    }
}
