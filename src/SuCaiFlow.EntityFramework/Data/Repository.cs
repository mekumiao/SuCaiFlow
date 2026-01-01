using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Contracts.Interfaces;

namespace SuCaiFlow.EntityFramework.Data;

public class Repository<T>(SuCaiFlowDbContext context) : IRepository<T> where T : class {
    protected readonly SuCaiFlowDbContext _context = context;

    public virtual async Task<T> AddAsync(T entity) {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id) {
        return await _context.Set<T>().FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync() {
        return await _context.Set<T>().ToListAsync();
    }

    public virtual async Task<T> UpdateAsync(T entity) {
        _context.Set<T>().Update(entity);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(Guid id) {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity != null) {
            _context.Set<T>().Remove(entity);
            return true;
        }
        return false;
    }

    public virtual async Task<int> SaveChangesAsync() {
        return await _context.SaveChangesAsync();
    }
}
