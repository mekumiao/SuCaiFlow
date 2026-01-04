using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public class SuCaiFlowAssetManager<TAsset>(ISuCaiFlowAssetStore<TAsset> assetStore) : ISuCaiFlowAssetManager
    where TAsset : class {
    private readonly ISuCaiFlowAssetStore<TAsset> _assetStore = assetStore;

    public ValueTask<long> CountAsync(CancellationToken cancellationToken = default) {
        return _assetStore.CountAsync(cancellationToken);
    }

    public ValueTask<long> CountAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken = default) {
        return _assetStore.CountAsync(query, cancellationToken);
    }

    public ValueTask<object> CreateAsync(SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask CreateAsync(object asset, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask DeleteAsync(object asset, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask<object?> FindByIdAsync(string identifier, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask<TResult?> GetAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<object>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<object> ListAsync(int? count = null, int? offset = null, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResult> ListAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<object>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask PopulateAsync(SuCaiFlowAssetDescriptor descriptor, object asset, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask PopulateAsync(object asset, SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask UpdateAsync(object asset, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask UpdateAsync(object asset, SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }
}
