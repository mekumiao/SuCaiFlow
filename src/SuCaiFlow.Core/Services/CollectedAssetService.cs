using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Repositories;
using SuCaiFlow.Contracts.Services;

namespace SuCaiFlow.Core.Services;

public class CollectedAssetService(ICollectedAssetRepository assetRepository) : ICollectedAssetService {
    private readonly ICollectedAssetRepository _assetRepository = assetRepository;

    public async Task<CollectedAsset?> GetAssetByIdAsync(Guid assetId, CancellationToken cancellationToken) {
        return await _assetRepository.GetByIdAsync(assetId, cancellationToken);
    }

    public async Task<IEnumerable<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken) {
        return await _assetRepository.GetAssetsByTaskIdAsync(taskId, cancellationToken);
    }

    public async Task<IEnumerable<CollectedAsset>> GetAllAssetsAsync(CancellationToken cancellationToken) {
        return await _assetRepository.GetAllAsync(cancellationToken);
    }

    public async Task<bool> DeleteAssetByIdAsync(Guid assetId, CancellationToken cancellationToken) {
        var result = await _assetRepository.DeleteAsync(assetId, cancellationToken);
        if (result) {
            await _assetRepository.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    public async Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken) {
        var result = await _assetRepository.DeleteAssetsByTaskIdAsync(taskId, cancellationToken);
        if (result) {
            await _assetRepository.SaveChangesAsync(cancellationToken);
        }
        return result;
    }
}
