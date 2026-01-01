using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Interfaces;

namespace SuCaiFlow.EntityFramework.Data;

public class CollectionTaskConfigRepository(SuCaiFlowDbContext context) : ICollectionTaskConfigRepository {
    private readonly SuCaiFlowDbContext _context = context;

    public async Task<CollectionTaskConfig> AddAsync(CollectionTaskConfig entity) {
        var dbSet = _context.CollectionTaskConfigs;
        await dbSet.AddAsync(entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id) {
        var dbSet = _context.CollectionTaskConfigs;
        var entity = await dbSet.FindAsync(id);
        if (entity != null) {
            dbSet.Remove(entity);
            return true;
        }
        return false;
    }

    public async Task<IEnumerable<CollectionTaskConfig>> GetAllAsync() {
        var dbSet = _context.CollectionTaskConfigs;
        return await dbSet.ToListAsync();
    }

    public async Task<CollectionTaskConfig?> GetByIdAsync(Guid id) {
        var dbSet = _context.CollectionTaskConfigs;
        return await dbSet.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CollectionTaskConfig> UpdateAsync(CollectionTaskConfig entity) {
        var dbSet = _context.CollectionTaskConfigs;
        dbSet.Update(entity);
        return entity;
    }

    public async Task<int> SaveChangesAsync() {
        return await _context.SaveChangesAsync();
    }

    public async Task<CollectionTaskConfig?> GetByNameAsync(string name) {
        var dbSet = _context.CollectionTaskConfigs;
        return await dbSet.FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<CollectionTaskConfig?> GetBySiteUrlAsync(string siteUrl) {
        var dbSet = _context.CollectionTaskConfigs;
        return await dbSet.FirstOrDefaultAsync(c => c.SiteUrl == siteUrl);
    }

    public async Task<IEnumerable<CollectionTaskConfig>> GetBySiteDomainAsync(string domain) {
        var dbSet = _context.CollectionTaskConfigs;
        return await dbSet.Where(c => c.SiteUrl!.Contains(domain)).ToListAsync();
    }
}
