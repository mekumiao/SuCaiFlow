using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.Contracts.Interfaces
{
    public interface ICollectionTaskConfigRepository : IRepository<CollectionTaskConfig>
    {
        Task<CollectionTaskConfig?> GetByNameAsync(string name);
        Task<CollectionTaskConfig?> GetBySiteUrlAsync(string siteUrl);
        Task<IEnumerable<CollectionTaskConfig>> GetBySiteDomainAsync(string domain);
    }
}
