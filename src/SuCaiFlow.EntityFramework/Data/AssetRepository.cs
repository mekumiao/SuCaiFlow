using Microsoft.EntityFrameworkCore;
using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Interfaces;

namespace SuCaiFlow.EntityFramework.Data
{
    public class AssetRepository : Repository<CollectedAsset>, IAssetRepository
    {
        public AssetRepository(SuCaiFlowDbContext context) : base(context)
        {
        }

        public async Task<List<CollectedAsset>> GetAssetsByTaskIdAsync(Guid taskId)
        {
            return await _context.CollectedAssets
                .Where(a => a.CollectionTaskId == taskId)
                .ToListAsync();
        }

        public async Task<List<CollectedAsset>> GetAssetsByStatusAsync(AssetStatus status)
        {
            return await _context.CollectedAssets
                .Where(a => a.Status == status)
                .ToListAsync();
        }

        public async Task<CollectedAsset?> GetByUrlAsync(string url)
        {
            return await _context.CollectedAssets
                .FirstOrDefaultAsync(a => a.Url == url);
        }
    }
}
