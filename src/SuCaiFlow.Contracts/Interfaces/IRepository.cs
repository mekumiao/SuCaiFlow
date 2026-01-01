namespace SuCaiFlow.Contracts.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task<T?> GetByIdAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(object id);
        Task<int> SaveChangesAsync();
    }
}
