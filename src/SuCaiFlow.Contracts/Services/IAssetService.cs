using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Services
{
    public interface IAssetService
    {
        Task<CollectedAsset?> GetAssetByIdAsync(Guid assetId);
        Task<List<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId);
        Task<List<CollectedAsset>> GetAllAssetsAsync();
        Task<bool> DeleteAssetAsync(Guid assetId);
        Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId);
    }
}
