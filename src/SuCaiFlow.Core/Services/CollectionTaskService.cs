using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Interfaces;
using SuCaiFlow.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace SuCaiFlow.Core.Services
{
    public class CollectionTaskService : ICollectionTaskService
    {
        private readonly ICollectionTaskRepository _collectionTaskRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly ITaskExecutionService _taskExecutionService;
        private readonly ILogger<CollectionTaskService> _logger;

        public CollectionTaskService(
            ICollectionTaskRepository collectionTaskRepository, 
            IAssetRepository assetRepository,
            ITaskExecutionService taskExecutionService,
            ILogger<CollectionTaskService> logger)
        {
            _collectionTaskRepository = collectionTaskRepository;
            _assetRepository = assetRepository;
            _taskExecutionService = taskExecutionService;
            _logger = logger;
        }

        public async Task<Guid> CreateCollectionTaskAsync(string name, string description, string url, string selector, Dictionary<string, string> parameters, int maxConcurrency = 5)
        {
            var collectionTask = new CollectionTask
            {
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

            await _collectionTaskRepository.AddAsync(collectionTask);
            await _collectionTaskRepository.SaveChangesAsync();

            _logger.LogInformation("Created collection task with ID: {TaskId}", collectionTask.Id);

            return collectionTask.Id;
        }

        public async Task<CollectionTask?> GetCollectionTaskByIdAsync(Guid taskId)
        {
            return await _collectionTaskRepository.GetByIdAsync(taskId);
        }

        public async Task<List<CollectionTask>> GetAllCollectionTasksAsync()
        {
            var tasks = await _collectionTaskRepository.GetAllAsync();
            return tasks.ToList();
        }

        public async Task<bool> StartCollectionTaskAsync(Guid taskId)
        {
            var task = await _collectionTaskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                _logger.LogWarning("Attempted to start non-existent task with ID: {TaskId}", taskId);
                return false;
            }

            // 检查任务是否已经在运行
            if (await _taskExecutionService.IsTaskRunningAsync(taskId))
            {
                _logger.LogWarning("Task with ID: {TaskId} is already running", taskId);
                return false;
            }

            // 更新任务状态为进行中
            task.Status = CollectionTaskStatus.InProgress;
            task.StartedAt = DateTime.UtcNow;
            await _collectionTaskRepository.UpdateAsync(task);
            await _collectionTaskRepository.SaveChangesAsync();

            // 标记任务为正在运行
            await _taskExecutionService.MarkTaskAsRunningAsync(taskId);

            _logger.LogInformation("Started collection task with ID: {TaskId}", taskId);

            return true;
        }

        public async Task<bool> CancelCollectionTaskAsync(Guid taskId)
        {
            var task = await _collectionTaskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                return false;
            }

            if (task.Status == CollectionTaskStatus.InProgress)
            {
                // 如果任务正在运行，需要先标记为完成
                await _taskExecutionService.MarkTaskAsCompletedAsync(taskId);
            }

            task.Status = CollectionTaskStatus.Cancelled;
            await _collectionTaskRepository.UpdateAsync(task);
            await _collectionTaskRepository.SaveChangesAsync();

            _logger.LogInformation("Cancelled collection task with ID: {TaskId}", taskId);

            return true;
        }

        public async Task<bool> DeleteCollectionTaskAsync(Guid taskId)
        {
            var task = await _collectionTaskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                return false;
            }

            // 删除相关的素材
            await _assetRepository.DeleteAssetsByTaskIdAsync(taskId);

            var result = await _collectionTaskRepository.DeleteAsync(taskId);
            if (result)
            {
                await _collectionTaskRepository.SaveChangesAsync();
                _logger.LogInformation("Deleted collection task with ID: {TaskId}", taskId);
            }

            return result;
        }

        public async Task<bool> UpdateTaskProgressAsync(Guid taskId, int assetsCollected, int totalExpected)
        {
            var task = await _collectionTaskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                _logger.LogWarning("Attempted to update progress for non-existent task with ID: {TaskId}", taskId);
                return false;
            }

            task.AssetsCollectedCount = assetsCollected;
            task.TotalAssetsExpected = totalExpected;

            await _collectionTaskRepository.UpdateAsync(task);
            await _collectionTaskRepository.SaveChangesAsync();

            _logger.LogInformation("Updated progress for task {TaskId}: {AssetsCollected}/{TotalExpected}", taskId, assetsCollected, totalExpected);

            return true;
        }
    }
}
