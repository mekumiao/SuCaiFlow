using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Repositories;

public interface ICollectedAssetRepository : IRepository<CollectedAsset> {
    Task<IEnumerable<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<IEnumerable<CollectedAsset>> GetAssetsByStatusAsync(CollectedAssetStatus status, CancellationToken cancellationToken);
    Task<CollectedAsset?> GetByUrlAsync(string url, CancellationToken cancellationToken);
    Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
}
