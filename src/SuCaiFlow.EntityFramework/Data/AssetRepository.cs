using Microsoft.EntityFrameworkCore;
using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Interfaces;

namespace SuCaiFlow.EntityFramework.Data
{
    public class AssetRepository : IAssetRepository
    {
        private readonly SuCaiFlowDbContext _context;

        public AssetRepository(SuCaiFlowDbContext context)
        {
            _context = context;
        }

        public async Task<CollectedAsset> AddAsync(CollectedAsset entity)
        {
            var dbSet = _context.CollectedAssets;
            await dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<bool> DeleteAsync(object id)
        {
            var dbSet = _context.CollectedAssets;
            var asset = await dbSet.FindAsync(id);
            if (asset != null)
            {
                dbSet.Remove(asset);
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<CollectedAsset>> GetAllAsync()
        {
            var dbSet = _context.CollectedAssets;
            return await dbSet.ToListAsync();
        }

        public async Task<CollectedAsset?> GetByIdAsync(object id)
        {
            var dbSet = _context.CollectedAssets;
            return await dbSet.FirstOrDefaultAsync(a => a.Id == (Guid)id);
        }

        public async Task<CollectedAsset> UpdateAsync(CollectedAsset entity)
        {
            var dbSet = _context.CollectedAssets;
            dbSet.Update(entity);
            return entity;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<List<CollectedAsset>> GetByTaskIdAsync(Guid taskId)
        {
            var dbSet = _context.CollectedAssets;
            return await dbSet.Where(a => a.CollectionTaskId == taskId).ToListAsync();
        }

        public async Task<List<CollectedAsset>> GetByTaskIdWithStatusAsync(Guid taskId, CollectionTaskStatus status)
        {
            var dbSet = _context.CollectedAssets;
            return await dbSet
                .Include(a => a.CollectionTask)
                .Where(a => a.CollectionTaskId == taskId && a.CollectionTask!.Status == status)
                .ToListAsync();
        }

        public async Task<bool> ExistsByTaskIdAsync(Guid taskId)
        {
            var dbSet = _context.CollectedAssets;
            return await dbSet.AnyAsync(a => a.CollectionTaskId == taskId);
        }

        public async Task<bool> DeleteAssetsByTaskIdAsync(Guid taskId)
        {
            var dbSet = _context.CollectedAssets;
            var assets = dbSet.Where(a => a.CollectionTaskId == taskId);
            var count = assets.Count();
            if (count > 0)
            {
                dbSet.RemoveRange(assets);
                return true;
            }
            return false;
        }

        public async Task<CollectedAsset?> GetByUrlAsync(string url)
        {
            var dbSet = _context.CollectedAssets;
            return await dbSet.FirstOrDefaultAsync(a => a.Url == url);
        }
    }
}
