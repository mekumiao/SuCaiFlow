using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Interfaces;

public interface IAssetService {
    Task<CollectedAsset?> GetAssetByIdAsync(Guid assetId);
    Task<IEnumerable<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId);
    Task<IEnumerable<CollectedAsset>> GetAllAssetsAsync();
    Task<bool> DeleteAssetByIdAsync(Guid assetId);
    Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId);
}
