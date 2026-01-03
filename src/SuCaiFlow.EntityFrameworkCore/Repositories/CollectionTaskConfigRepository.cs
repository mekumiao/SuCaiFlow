using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Repositories;

namespace SuCaiFlow.EntityFrameworkCore.Repositories;

public class CollectionTaskConfigRepository(ISuCaiFlowEntityFrameworkCoreContext context)
    : Repository<CollectionTaskConfig>(context), ICollectionTaskConfigRepository {

    public async Task<CollectionTaskConfig?> GetByNameAsync(string name, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        var dbSet = context.Set<CollectionTaskConfig>();
        return await dbSet.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<CollectionTaskConfig?> GetBySiteUrlAsync(string siteUrl, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        var dbSet = context.Set<CollectionTaskConfig>();
        return await dbSet.FirstOrDefaultAsync(c => c.SiteUrl == siteUrl, cancellationToken);
    }

    public async Task<IEnumerable<CollectionTaskConfig>> GetBySiteDomainAsync(string domain, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        var dbSet = context.Set<CollectionTaskConfig>();
        return await dbSet.Where(c => c.SiteUrl!.Contains(domain)).ToListAsync(cancellationToken);
    }
}
