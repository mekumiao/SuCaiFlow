using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Services;

public interface ICollectedAssetService {
    Task<CollectedAsset?> GetAssetByIdAsync(Guid assetId, CancellationToken cancellationToken);
    Task<IEnumerable<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<IEnumerable<CollectedAsset>> GetAllAssetsAsync(CancellationToken cancellationToken);
    Task<bool> DeleteAssetByIdAsync(Guid assetId, CancellationToken cancellationToken);
    Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
}
