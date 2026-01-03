using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Contracts.Repositories;

namespace SuCaiFlow.EntityFrameworkCore.Repositories;

public class Repository<T>(ISuCaiFlowEntityFrameworkCoreContext context) : IRepository<T> where T : class {
    protected readonly ISuCaiFlowEntityFrameworkCoreContext _context = context;

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        await context.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        return await context.Set<T>().FindAsync([id], cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        return await context.Set<T>().ToListAsync(cancellationToken);
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        context.Set<T>().Update(entity);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        var entity = await context.Set<T>().FindAsync([id], cancellationToken);
        if (entity != null) {
            context.Set<T>().Remove(entity);
            return true;
        }
        return false;
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}
