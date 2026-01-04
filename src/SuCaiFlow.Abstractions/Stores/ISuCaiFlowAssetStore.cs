namespace SuCaiFlow.Abstractions;

public interface ISuCaiFlowAssetStore<TAsset> where TAsset : class {
    ValueTask<long> CountAsync(CancellationToken cancellationToken);
    ValueTask<long> CountAsync<TResult>(Func<IQueryable<TAsset>, IQueryable<TResult>> query, CancellationToken cancellationToken);
    ValueTask<TAsset> CreateAsync(TAsset entity, CancellationToken cancellationToken);
    ValueTask<TAsset?> FindByIdAsync(string identifier, CancellationToken cancellationToken);
    ValueTask<TAsset?> FindByAssetIdAsync(string identifier, CancellationToken cancellationToken);
    ValueTask<TResult?> GetAsync<TState, TResult>(
        Func<IQueryable<TAsset>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken);
    ValueTask<TAsset> InstantiateAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<TAsset> ListAsync(int? count, int? offset, CancellationToken cancellationToken);
    IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
        Func<IQueryable<TAsset>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken);
    ValueTask UpdateAsync(TAsset application, CancellationToken cancellationToken);
    ValueTask DeleteAsync(TAsset application, CancellationToken cancellationToken);
    ValueTask DeleteByTaskIdAsync(string identifier, CancellationToken cancellationToken);
    IAsyncEnumerable<TAsset> FindByTaskIdAsync(string identifier, CancellationToken cancellationToken);
    IAsyncEnumerable<TAsset> FindByStatusAsync(string status, CancellationToken cancellationToken);
}
