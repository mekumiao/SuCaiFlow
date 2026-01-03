using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Repositories;

public interface ICollectionTaskConfigRepository : IRepository<CollectionTaskConfig> {
    Task<CollectionTaskConfig?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<CollectionTaskConfig?> GetBySiteUrlAsync(string siteUrl, CancellationToken cancellationToken);
    Task<IEnumerable<CollectionTaskConfig>> GetBySiteDomainAsync(string domain, CancellationToken cancellationToken);
}
