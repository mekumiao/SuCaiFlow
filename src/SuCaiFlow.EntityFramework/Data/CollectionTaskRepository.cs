using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Interfaces;

namespace SuCaiFlow.EntityFramework.Data;

public class CollectionTaskRepository(SuCaiFlowDbContext context) : ICollectionTaskRepository {
    private readonly SuCaiFlowDbContext _context = context;

    public async Task<CollectionTask> AddAsync(CollectionTask entity) {
        var dbSet = _context.CollectionTasks;
        await dbSet.AddAsync(entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id) {
        var dbSet = _context.CollectionTasks;
        var task = await dbSet.FindAsync(id);
        if (task != null) {
            dbSet.Remove(task);
            return true;
        }
        return false;
    }

    public async Task<IEnumerable<CollectionTask>> GetAllAsync() {
        var dbSet = _context.CollectionTasks;
        return await dbSet.ToListAsync();
    }

    public async Task<CollectionTask?> GetByIdAsync(Guid id) {
        var dbSet = _context.CollectionTasks;
        return await dbSet
            .Include(t => t.Assets)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<CollectionTask> UpdateAsync(CollectionTask entity) {
        var dbSet = _context.CollectionTasks;
        dbSet.Update(entity);
        return entity;
    }

    public async Task<int> SaveChangesAsync() {
        return await _context.SaveChangesAsync();
    }

    public async Task<CollectionTask?> GetByStatusAsync(CollectionTaskStatus status) {
        var dbSet = _context.CollectionTasks;
        return await dbSet
            .Include(t => t.Assets)
            .FirstOrDefaultAsync(t => t.Status == status);
    }

    public async Task<IEnumerable<CollectionTask>> GetByStatusListAsync(List<CollectionTaskStatus> statuses) {
        var dbSet = _context.CollectionTasks;
        return await dbSet
            .Include(t => t.Assets)
            .Where(t => statuses.Contains(t.Status))
            .ToListAsync();
    }

    public async Task<bool> ExistsWithStatusAsync(Guid taskId, CollectionTaskStatus status) {
        var dbSet = _context.CollectionTasks;
        return await dbSet.AnyAsync(t => t.Id == taskId && t.Status == status);
    }
}
