using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Interfaces;

namespace SuCaiFlow.Core.Services;

public class AssetService(IAssetRepository assetRepository) : IAssetService {
    private readonly IAssetRepository _assetRepository = assetRepository;

    public async Task<CollectedAsset?> GetAssetByIdAsync(Guid assetId) {
        return await _assetRepository.GetByIdAsync(assetId);
    }

    public async Task<IEnumerable<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId) {
        return await _assetRepository.GetAssetsByTaskIdAsync(taskId);
    }

    public async Task<IEnumerable<CollectedAsset>> GetAllAssetsAsync() {
        return await _assetRepository.GetAllAsync();
    }

    public async Task<bool> DeleteAssetByIdAsync(Guid assetId) {
        var result = await _assetRepository.DeleteAsync(assetId);
        if (result) {
            await _assetRepository.SaveChangesAsync();
        }
        return result;
    }

    public async Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId) {
        var result = await _assetRepository.DeleteAssetsByTaskIdAsync(taskId);
        if (result) {
            await _assetRepository.SaveChangesAsync();
        }
        return result;
    }
}
