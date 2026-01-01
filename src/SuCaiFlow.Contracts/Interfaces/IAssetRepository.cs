using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Interfaces
{
    public interface IAssetRepository : IRepository<CollectedAsset>
    {
        Task<List<CollectedAsset>> GetByTaskIdAsync(Guid taskId);
        Task<List<CollectedAsset>> GetByTaskIdWithStatusAsync(Guid taskId, CollectionTaskStatus status);
        Task<bool> ExistsByTaskIdAsync(Guid taskId);
        Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId);
        Task<CollectedAsset?> GetByUrlAsync(string url);
    }
}
