using Microsoft.Extensions.Logging;

namespace SuCaiFlow.Engine;

/// <summary>
/// 任务执行器 - 管理任务的运行状态
/// </summary>
/// <param name="logger"></param>
public class SuCaiFlowEngineTaskExecution(ILogger<SuCaiFlowEngineTaskExecution> logger) {
    private readonly ILogger<SuCaiFlowEngineTaskExecution> _logger = logger;
    private readonly HashSet<Guid> _runningTasks = [];

    public bool IsTaskRunning(Guid taskId) {
        var isRunning = _runningTasks.Contains(taskId);
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("检查任务 {TaskId} 运行状态: {IsRunning}", taskId, isRunning);
        return isRunning;
    }

    public bool MarkTaskAsRunning(Guid taskId) {
        if (_runningTasks.Add(taskId)) {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("标记任务 {TaskId} 为运行状态", taskId);
            return true;
        }
        if (_logger.IsEnabled(LogLevel.Warning))
            _logger.LogWarning("任务 {TaskId} 已在运行状态", taskId);
        return false;
    }

    public bool MarkTaskAsCompleted(Guid taskId) {
        if (_runningTasks.Remove(taskId)) {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("标记任务 {TaskId} 为完成状态", taskId);
            return true;
        }
        if (_logger.IsEnabled(LogLevel.Warning))
            _logger.LogWarning("任务 {TaskId} 不在运行状态", taskId);
        return false;
    }

    public bool MarkTaskAsFailed(Guid taskId) {
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
