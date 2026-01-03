using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Repositories;

namespace SuCaiFlow.EntityFrameworkCore.Repositories;

public class CollectionTaskRepository(ISuCaiFlowEntityFrameworkCoreContext context)
    : Repository<CollectionTask>(context), ICollectionTaskRepository {

    public async Task<CollectionTask?> GetByStatusAsync(CollectionTaskStatus status, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        var dbSet = context.Set<CollectionTask>();
        return await dbSet
            .Include(t => t.Assets)
            .FirstOrDefaultAsync(t => t.Status == status, cancellationToken);
    }

    public async Task<IEnumerable<CollectionTask>> GetByStatusListAsync(IEnumerable<CollectionTaskStatus> statuses, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        var dbSet = context.Set<CollectionTask>();
        return await dbSet
            .Include(t => t.Assets)
            .Where(t => statuses.Contains(t.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithStatusAsync(Guid taskId, CollectionTaskStatus status, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        var dbSet = context.Set<CollectionTask>();
        return await dbSet.AnyAsync(t => t.Id == taskId && t.Status == status, cancellationToken);
    }
}
