using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public class SuCaiFlowAssetManager<TAsset>(ISuCaiFlowAssetStore<TAsset> store) : ISuCaiFlowAssetManager
    where TAsset : class {

    protected ISuCaiFlowAssetStore<TAsset> Store { get; } = store ?? throw new ArgumentNullException(nameof(store));

    #region 自身实现
    public virtual ValueTask<long> CountAsync(CancellationToken cancellationToken = default) {
        return Store.CountAsync(cancellationToken);
    }

    public virtual ValueTask<long> CountAsync<TResult>(Func<IQueryable<TAsset>, IQueryable<TResult>> query, CancellationToken cancellationToken = default) {
        return Store.CountAsync(query, cancellationToken);
    }

    public virtual async ValueTask<TAsset> CreateAsync(SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(descriptor);

        var entity = await Store.InstantiateAsync(cancellationToken) ??
            throw new InvalidOperationException("无法创建实例");

        await PopulateAsync(entity, descriptor, cancellationToken);
        await CreateAsync(entity, cancellationToken);

        return entity;
    }

    public virtual async ValueTask<IReadOnlyList<object>> CreateRangeAsync(ICollection<SuCaiFlowAssetDescriptor> descriptors, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(descriptors);

        var entities = new List<TAsset>();

        foreach (var descriptor in descriptors) {
            var entity = await Store.InstantiateAsync(cancellationToken) ??
                throw new InvalidOperationException("无法创建实例");
            await PopulateAsync(entity, descriptor, cancellationToken);
            entities.Add(entity);
        }

        await Store.CreateRangeAsync(entities, cancellationToken);

        return entities;
    }

    public virtual async ValueTask CreateRangeAsync(ICollection<TAsset> assets, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(assets);
        await Store.CreateRangeAsync(assets, cancellationToken);
    }

    public virtual async ValueTask CreateAsync(TAsset asset, CancellationToken cancellationToken = default) {
        await Store.CreateAsync(asset, cancellationToken);
    }

    public virtual async ValueTask DeleteAsync(TAsset asset, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(asset);

        await Store.DeleteAsync(asset, cancellationToken);
    }

    public virtual async ValueTask<TAsset?> FindByIdAsync(string identifier, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        var asset = await Store.FindByIdAsync(identifier, cancellationToken);

        return asset is null ? null : asset;
    }

    public virtual ValueTask<TResult?> GetAsync<TResult>(Func<IQueryable<TAsset>, IQueryable<TResult>> query, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(query);

        return GetAsync(static (tasks, query) => query(tasks), query, cancellationToken);
    }

    public virtual ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<TAsset>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(query);

        return Store.GetAsync(query, state, cancellationToken);
    }

    public virtual IAsyncEnumerable<TAsset> ListAsync(int? count = null, int? offset = null, CancellationToken cancellationToken = default) {
        return Store.ListAsync(count, offset, cancellationToken);
    }

    public virtual IAsyncEnumerable<TResult> ListAsync<TResult>(Func<IQueryable<TAsset>, IQueryable<TResult>> query, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(query);

        return ListAsync(static (assets, query) => query(assets), query, cancellationToken);
    }

    public virtual IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<TAsset>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(query);

        return Store.ListAsync(query, state, cancellationToken);
    }

    public virtual async ValueTask PopulateAsync(SuCaiFlowAssetDescriptor descriptor, TAsset asset, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(asset);

        descriptor.TaskId = await Store.GetTaskIdAsync(asset, cancellationToken);
        descriptor.OrderNo = await Store.GetOrderNoAsync(asset, cancellationToken);
        descriptor.Name = await Store.GetNameAsync(asset, cancellationToken);
        descriptor.Title = await Store.GetTitleAsync(asset, cancellationToken);
        descriptor.Description = await Store.GetDescriptionAsync(asset, cancellationToken);
        descriptor.OriginalUrl = await Store.GetOriginalUrlAsync(asset, cancellationToken);
        descriptor.LandingUrl = await Store.GetLandingUrlAsync(asset, cancellationToken);
        descriptor.ObjectKey = await Store.GetObjectKeyAsync(asset, cancellationToken);
        descriptor.ContentType = await Store.GetContentTypeAsync(asset, cancellationToken);
        descriptor.Status = await Store.GetStatusAsync(asset, cancellationToken);
        descriptor.Size = await Store.GetSizeAsync(asset, cancellationToken);
        descriptor.CreatedAt = await Store.GetCreatedAtAsync(asset, cancellationToken);
        descriptor.DownloadedAt = await Store.GetDownloadedAtAsync(asset, cancellationToken);
    }

    public virtual async ValueTask PopulateAsync(TAsset asset, SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(descriptor);

        await Store.SetTaskIdAsync(asset, descriptor.TaskId, cancellationToken);
        await Store.SetOrderNoAsync(asset, descriptor.OrderNo, cancellationToken);
        await Store.SetNameAsync(asset, descriptor.Name, cancellationToken);
        await Store.SetTitleAsync(asset, descriptor.Title, cancellationToken);
        await Store.SetDescriptionAsync(asset, descriptor.Description, cancellationToken);
        await Store.SetOriginalUrlAsync(asset, descriptor.OriginalUrl, cancellationToken);
        await Store.SetLandingUrlAsync(asset, descriptor.LandingUrl, cancellationToken);
        await Store.SetObjectKeyAsync(asset, descriptor.ObjectKey, cancellationToken);
        await Store.SetContentTypeAsync(asset, descriptor.ContentType, cancellationToken);
        await Store.SetStatusAsync(asset, descriptor.Status, cancellationToken);
        await Store.SetSizeAsync(asset, descriptor.Size, cancellationToken);
        await Store.SetCreatedAtAsync(asset, descriptor.CreatedAt, cancellationToken);
        await Store.SetDownloadedAtAsync(asset, descriptor.DownloadedAt, cancellationToken);
    }

    public virtual async ValueTask UpdateAsync(TAsset asset, SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(descriptor);

        await PopulateAsync(asset, descriptor, cancellationToken);
        await UpdateAsync(asset, cancellationToken);
    }

    public virtual async ValueTask UpdateAsync(TAsset asset, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(asset);

        await Store.UpdateAsync(asset, cancellationToken);
    }

    public virtual ValueTask<string?> GetIdAsync(TAsset asset, CancellationToken cancellationToken = default) {
        return Store.GetIdAsync(asset, cancellationToken);
    }
    #endregion

    #region 接口实现
    ValueTask<long> ISuCaiFlowAssetManager.CountAsync(CancellationToken cancellationToken) {
        return CountAsync(cancellationToken);
    }

    ValueTask<long> ISuCaiFlowAssetManager.CountAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken) {
        return CountAsync(query, cancellationToken);
    }

    async ValueTask<object> ISuCaiFlowAssetManager.CreateAsync(SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken) {
        return await CreateAsync(descriptor, cancellationToken);
    }

    ValueTask ISuCaiFlowAssetManager.CreateAsync(object asset, CancellationToken cancellationToken) {
        return CreateAsync((TAsset)asset, cancellationToken);
    }

    ValueTask<IReadOnlyList<object>> ISuCaiFlowAssetManager.CreateRangeAsync(ICollection<SuCaiFlowAssetDescriptor> descriptors, CancellationToken cancellationToken) {
        return CreateRangeAsync(descriptors, cancellationToken);
    }

    ValueTask ISuCaiFlowAssetManager.CreateRangeAsync(ICollection<object> assets, CancellationToken cancellationToken) {
        return CreateRangeAsync((ICollection<TAsset>)assets, cancellationToken);
    }

    ValueTask ISuCaiFlowAssetManager.DeleteAsync(object asset, CancellationToken cancellationToken) {
        return DeleteAsync((TAsset)asset, cancellationToken);
    }

    async ValueTask<object?> ISuCaiFlowAssetManager.FindByIdAsync(string identifier, CancellationToken cancellationToken) {
        return await FindByIdAsync(identifier, cancellationToken);
    }

    ValueTask<TResult?> ISuCaiFlowAssetManager.GetAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken) where TResult : default {
        return GetAsync(query, cancellationToken);
    }

    ValueTask<TResult?> ISuCaiFlowAssetManager.GetAsync<TState, TResult>(Func<IQueryable<object>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) where TResult : default {
        return GetAsync(query, state, cancellationToken);
    }

    IAsyncEnumerable<object> ISuCaiFlowAssetManager.ListAsync(int? count, int? offset, CancellationToken cancellationToken) {
        return ListAsync(count, offset, cancellationToken);
    }

    IAsyncEnumerable<TResult> ISuCaiFlowAssetManager.ListAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken) {
        return ListAsync(query, cancellationToken);
    }

    IAsyncEnumerable<TResult> ISuCaiFlowAssetManager.ListAsync<TState, TResult>(Func<IQueryable<object>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) {
        return ListAsync(query, state, cancellationToken);
    }

    ValueTask ISuCaiFlowAssetManager.PopulateAsync(SuCaiFlowAssetDescriptor descriptor, object asset, CancellationToken cancellationToken) {
        return PopulateAsync(descriptor, (TAsset)asset, cancellationToken);
    }

    ValueTask ISuCaiFlowAssetManager.PopulateAsync(object asset, SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken) {
        return PopulateAsync((TAsset)asset, descriptor, cancellationToken);
    }

    ValueTask ISuCaiFlowAssetManager.UpdateAsync(object asset, CancellationToken cancellationToken) {
        return UpdateAsync((TAsset)asset, cancellationToken);
    }

    ValueTask ISuCaiFlowAssetManager.UpdateAsync(object asset, SuCaiFlowAssetDescriptor descriptor, CancellationToken cancellationToken) {
        return UpdateAsync((TAsset)asset, descriptor, cancellationToken);
    }

    ValueTask<string?> ISuCaiFlowAssetManager.GetIdAsync(object asset, CancellationToken cancellationToken) {
        return GetIdAsync((TAsset)asset, cancellationToken);
    }
    #endregion
}
