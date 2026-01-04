namespace SuCaiFlow.Abstractions;

public interface ISuCaiFlowTaskStore<TTask> where TTask : class {
    ValueTask<long> CountAsync(CancellationToken cancellationToken);
    ValueTask<long> CountAsync<TResult>(Func<IQueryable<TTask>, IQueryable<TResult>> query, CancellationToken cancellationToken);
    ValueTask<TTask> CreateAsync(TTask entity, CancellationToken cancellationToken);
    ValueTask<TTask?> FindByIdAsync(string identifier, CancellationToken cancellationToken);
    ValueTask<TTask?> FindByAssetIdAsync(string identifier, CancellationToken cancellationToken);
    ValueTask<TResult?> GetAsync<TState, TResult>(
        Func<IQueryable<TTask>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken);
    ValueTask<TTask> InstantiateAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<TTask> ListAsync(int? count, int? offset, CancellationToken cancellationToken);
    IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
        Func<IQueryable<TTask>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken);
    ValueTask UpdateAsync(TTask application, CancellationToken cancellationToken);
    ValueTask DeleteAsync(TTask application, CancellationToken cancellationToken);
    ValueTask DeleteByTaskIdAsync(string identifier, CancellationToken cancellationToken);
    IAsyncEnumerable<TTask> FindByTaskIdAsync(string identifier, CancellationToken cancellationToken);
    IAsyncEnumerable<TTask> FindByStatusAsync(string status, CancellationToken cancellationToken);
}
