using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Interfaces
{
    public interface IAssetRepository : IRepository<CollectedAsset>
    {
        Task<List<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId);
        Task<List<CollectedAsset>> GetAssetsByStatusAsync(AssetStatus status);
        Task<CollectedAsset?> GetByUrlAsync(string url);
    }
}
