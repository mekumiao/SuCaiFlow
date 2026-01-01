using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Interfaces;
using SuCaiFlow.Contracts.Services;

namespace SuCaiFlow.Core.Services
{
    public class AssetService : IAssetService
    {
        private readonly IAssetRepository _assetRepository;

        public AssetService(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        public async Task<CollectedAsset?> GetAssetByIdAsync(Guid assetId)
        {
            return await _assetRepository.GetByIdAsync(assetId);
        }

        public async Task<List<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId)
        {
            return await _assetRepository.GetByTaskIdAsync(taskId);
        }

        public async Task<List<CollectedAsset>> GetAllAssetsAsync()
        {
            return (await _assetRepository.GetAllAsync()).ToList();
        }

        public async Task<bool> DeleteAssetAsync(Guid assetId)
        {
            var result = await _assetRepository.DeleteAsync(assetId);
            if (result)
            {
                await _assetRepository.SaveChangesAsync();
            }
            return result;
        }

        public async Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId)
        {
            var result = await _assetRepository.DeleteAssetsByTaskIdAsync(taskId);
            if (result)
            {
                await _assetRepository.SaveChangesAsync();
            }
            return result;
        }
    }
}
