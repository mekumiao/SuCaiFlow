using Microsoft.Extensions.Logging;

using SuCaiFlow.Contracts.Interfaces;

namespace SuCaiFlow.Core.Services;

/// <summary>
/// 任务执行服务实现
/// </summary>
public class TaskExecutionService(ILogger<TaskExecutionService> logger) : ITaskExecutionService {
    private readonly ILogger<TaskExecutionService> _logger = logger;
    private readonly HashSet<Guid> _runningTasks = [];

    public async Task<bool> IsTaskRunningAsync(Guid taskId) {
        var isRunning = _runningTasks.Contains(taskId);
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("检查任务 {TaskId} 运行状态: {IsRunning}", taskId, isRunning);
        return isRunning;
    }

    public async Task<bool> MarkTaskAsRunningAsync(Guid taskId) {
        if (_runningTasks.Add(taskId)) {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("标记任务 {TaskId} 为运行状态", taskId);
            return true;
        }
        if (_logger.IsEnabled(LogLevel.Warning))
            _logger.LogWarning("任务 {TaskId} 已在运行状态", taskId);
        return false;
    }

    public async Task<bool> MarkTaskAsCompletedAsync(Guid taskId) {
        if (_runningTasks.Remove(taskId)) {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("标记任务 {TaskId} 为完成状态", taskId);
            return true;
        }
        if (_logger.IsEnabled(LogLevel.Warning))
            _logger.LogWarning("任务 {TaskId} 不在运行状态", taskId);
        return false;
    }

    public async Task<bool> MarkTaskAsFailedAsync(Guid taskId) {
        if (_runningTasks.Remove(taskId)) {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("标记任务 {TaskId} 为失败状态", taskId);
            return true;
        }
        if (_logger.IsEnabled(LogLevel.Warning))
            _logger.LogWarning("任务 {TaskId} 不在运行状态", taskId);
        return false;
    }
}
