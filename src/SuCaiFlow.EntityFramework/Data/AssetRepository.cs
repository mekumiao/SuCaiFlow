using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Interfaces;

namespace SuCaiFlow.EntityFramework.Data;

public class AssetRepository(SuCaiFlowDbContext context) : Repository<CollectedAsset>(context), IAssetRepository {
    public async Task<IEnumerable<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId) {
        return await _context.CollectedAssets
            .Where(a => a.CollectionTaskId == taskId)
            .ToListAsync();
    }

    public async Task<IEnumerable<CollectedAsset>> GetAssetsByStatusAsync(AssetStatus status) {
        return await _context.CollectedAssets
            .Where(a => a.Status == status)
            .ToListAsync();
    }

    public async Task<CollectedAsset?> GetByUrlAsync(string url) {
        return await _context.CollectedAssets
            .FirstOrDefaultAsync(a => a.Url == url);
    }

    public async Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId) {
        var assets = await _context.CollectedAssets
            .Where(a => a.CollectionTaskId == taskId)
            .ToListAsync();
        _context.CollectedAssets.RemoveRange(assets);
        return assets.Count > 0;
    }
}
