using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Interfaces
{
    public interface ICollectionTaskRepository : IRepository<CollectionTask>
    {
        Task<CollectionTask?> GetByStatusAsync(CollectionTaskStatus status);
        Task<List<CollectionTask>> GetByStatusListAsync(List<CollectionTaskStatus> statuses);
        Task<bool> ExistsWithStatusAsync(Guid taskId, CollectionTaskStatus status);
    }
}
