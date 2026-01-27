namespace SuCaiFlow.Abstractions;

public interface ISuCaiFlowAssetManager {
    ValueTask<long> CountAsync(CancellationToken cancellationToken = default);
    ValueTask<long> CountAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken = default);

    ValueTask<object> CreateAsync(SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken = default);
    ValueTask CreateAsync(object asset, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyList<object>> CreateRangeAsync(ICollection<SuCaiFlowAssetDescriptor> descriptors, CancellationToken cancellationToken = default);
    ValueTask CreateRangeAsync(ICollection<object> assets, CancellationToken cancellationToken = default);
    ValueTask DeleteAsync(object asset, CancellationToken cancellationToken = default);
    ValueTask PopulateAsync(SuCaiFlowAssetDescriptor descriptor, object asset, CancellationToken cancellationToken = default);
    ValueTask PopulateAsync(object asset, SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken = default);
    ValueTask UpdateAsync(object asset, CancellationToken cancellationToken = default);
    ValueTask UpdateAsync(object asset, SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken = default);

    ValueTask<object?> FindByIdAsync(string identifier, CancellationToken cancellationToken = default);
    ValueTask<TResult?> GetAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken = default);
    ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<object>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken = default);
    IAsyncEnumerable<object> ListAsync(int? count = null, int? offset = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TResult> ListAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<object>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken = default);

    ValueTask<string?> GetIdAsync(object asset, CancellationToken cancellationToken = default);
}
