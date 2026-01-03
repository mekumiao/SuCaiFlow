using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Services;

public interface ICollectionTaskService {
    Task<Guid> CreateCollectionTaskAsync(string name, string description, string url, string selector, Dictionary<string, string> parameters, int maxConcurrency, CancellationToken cancellationToken);
    Task<CollectionTask?> GetCollectionTaskByIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<IEnumerable<CollectionTask>> GetAllCollectionTasksAsync(CancellationToken cancellationToken);
    Task<bool> StartCollectionTaskAsync(Guid taskId, CancellationToken cancellationToken);
    Task<bool> CancelCollectionTaskAsync(Guid taskId, CancellationToken cancellationToken);
    Task<bool> DeleteCollectionTaskAsync(Guid taskId, CancellationToken cancellationToken);
}
