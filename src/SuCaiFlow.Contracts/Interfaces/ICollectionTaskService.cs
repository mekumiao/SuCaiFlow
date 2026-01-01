using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Interfaces
{
    public interface ICollectionTaskService
    {
        Task<Guid> CreateCollectionTaskAsync(string name, string description, string url, string selector, Dictionary<string, string> parameters, int maxConcurrency = 5);
        
        Task<CollectionTask?> GetCollectionTaskByIdAsync(Guid taskId);
        
        Task<List<CollectionTask>> GetAllCollectionTasksAsync();
        
        Task<bool> StartCollectionTaskAsync(Guid taskId);
        
        Task<bool> CancelCollectionTaskAsync(Guid taskId);
        
        Task<bool> DeleteCollectionTaskAsync(Guid taskId);
        
        Task<bool> UpdateTaskProgressAsync(Guid taskId, int assetsCollected, int totalExpected);
    }
}
