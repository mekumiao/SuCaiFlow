using Microsoft.Extensions.Logging;

using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Repositories;
using SuCaiFlow.Contracts.Services;

namespace SuCaiFlow.Core.Services;

public class CollectionTaskService(
    ICollectionTaskRepository collectionTaskRepository,
    ICollectedAssetRepository assetRepository,
    ITaskExecutionService taskExecutionService,
    ILogger<CollectionTaskService> logger) : ICollectionTaskService {
    private readonly ICollectionTaskRepository _collectionTaskRepository = collectionTaskRepository;
    private readonly ICollectedAssetRepository _assetRepository = assetRepository;
    private readonly ITaskExecutionService _taskExecutionService = taskExecutionService;
    private readonly ILogger<CollectionTaskService> _logger = logger;

    public async Task<Guid> CreateCollectionTaskAsync(string name, string description, string url, string selector, Dictionary<string, string> parameters, int maxConcurrency, CancellationToken cancellationToken) {
        var collectionTask = new CollectionTask {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Url = url,
            Selector = selector,
            Parameters = parameters,
            MaxConcurrency = maxConcurrency,
            Status = CollectionTaskStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _collectionTaskRepository.AddAsync(collectionTask, cancellationToken);
        await _collectionTaskRepository.SaveChangesAsync(cancellationToken);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Created collection task with ID: {TaskId}", collectionTask.Id);

        return collectionTask.Id;
    }

    public async Task<CollectionTask?> GetCollectionTaskByIdAsync(Guid taskId, CancellationToken cancellationToken) {
        return await _collectionTaskRepository.GetByIdAsync(taskId, cancellationToken);
    }

    public async Task<IEnumerable<CollectionTask>> GetAllCollectionTasksAsync(CancellationToken cancellationToken) {
        return await _collectionTaskRepository.GetAllAsync(cancellationToken);
    }

    public async Task<bool> StartCollectionTaskAsync(Guid taskId, CancellationToken cancellationToken) {
        var task = await _collectionTaskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null) {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Attempted to start non-existent task with ID: {TaskId}", taskId);
            return false;
        }

        // 检查任务是否已经在运行
        if (await _taskExecutionService.IsTaskRunningAsync(taskId, cancellationToken)) {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Task with ID: {TaskId} is already running", taskId);
            return false;
        }

        // 更新任务状态为进行中
        task.Status = CollectionTaskStatus.InProgress;
        task.StartedAt = DateTime.UtcNow;
        await _collectionTaskRepository.UpdateAsync(task, cancellationToken);
        await _collectionTaskRepository.SaveChangesAsync(cancellationToken);

        // 标记任务为正在运行
        await _taskExecutionService.MarkTaskAsRunningAsync(taskId, cancellationToken);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Started collection task with ID: {TaskId}", taskId);

        return true;
    }

    public async Task<bool> CancelCollectionTaskAsync(Guid taskId, CancellationToken cancellationToken) {
        var task = await _collectionTaskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null) {
            return false;
        }

        if (task.Status == CollectionTaskStatus.InProgress) {
            // 如果任务正在运行，需要先标记为完成
            await _taskExecutionService.MarkTaskAsCompletedAsync(taskId, cancellationToken);
        }

        task.Status = CollectionTaskStatus.Cancelled;
        await _collectionTaskRepository.UpdateAsync(task, cancellationToken);
        await _collectionTaskRepository.SaveChangesAsync(cancellationToken);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Cancelled collection task with ID: {TaskId}", taskId);

        return true;
    }

    public async Task<bool> DeleteCollectionTaskAsync(Guid taskId, CancellationToken cancellationToken) {
        var task = await _collectionTaskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null) {
            return false;
        }

        // 删除相关的素材
        await _assetRepository.DeleteAssetsByTaskIdAsync(taskId, cancellationToken);

        var result = await _collectionTaskRepository.DeleteAsync(taskId, cancellationToken);
        if (result) {
            await _collectionTaskRepository.SaveChangesAsync(cancellationToken);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Deleted collection task with ID: {TaskId}", taskId);
        }

        return result;
    }

    public async Task<bool> UpdateTaskProgressAsync(Guid taskId, int assetsCollected, int totalExpected, CancellationToken cancellationToken) {
        var task = await _collectionTaskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null) {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Attempted to update progress for non-existent task with ID: {TaskId}", taskId);
            return false;
        }

        task.AssetsCollectedCount = assetsCollected;
        task.TotalAssetsExpected = totalExpected;

        await _collectionTaskRepository.UpdateAsync(task, cancellationToken);
        await _collectionTaskRepository.SaveChangesAsync(cancellationToken);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Updated progress for task {TaskId}: {AssetsCollected}/{TotalExpected}", taskId, assetsCollected, totalExpected);

        return true;
    }
}
