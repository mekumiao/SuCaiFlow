using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Repositories;

namespace SuCaiFlow.EntityFrameworkCore.Repositories;

public class CollectedAssetRepository(ISuCaiFlowEntityFrameworkCoreContext context)
    : Repository<CollectedAsset>(context), ICollectedAssetRepository {
    public async Task<IEnumerable<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        return await context.Set<CollectedAsset>()
            .Where(a => a.CollectionTaskId == taskId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CollectedAsset>> GetAssetsByStatusAsync(CollectedAssetStatus status, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        return await context.Set<CollectedAsset>()
            .Where(a => a.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<CollectedAsset?> GetByUrlAsync(string url, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        return await context.Set<CollectedAsset>()
            .FirstOrDefaultAsync(a => a.Url == url, cancellationToken);
    }

    public async Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        var assets = await context.Set<CollectedAsset>()
            .Where(a => a.CollectionTaskId == taskId)
            .ToListAsync(cancellationToken);
        context.Set<CollectedAsset>().RemoveRange(assets);
        return assets.Count > 0;
    }
}
