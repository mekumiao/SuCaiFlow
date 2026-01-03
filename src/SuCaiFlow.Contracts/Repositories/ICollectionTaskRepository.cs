using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Repositories;

public interface ICollectionTaskRepository : IRepository<CollectionTask> {
    Task<CollectionTask?> GetByStatusAsync(CollectionTaskStatus status, CancellationToken cancellationToken);
    Task<IEnumerable<CollectionTask>> GetByStatusListAsync(IEnumerable<CollectionTaskStatus> statuses, CancellationToken cancellationToken);
    Task<bool> ExistsWithStatusAsync(Guid taskId, CollectionTaskStatus status, CancellationToken cancellationToken);
}
