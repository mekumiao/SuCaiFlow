using System.Collections.Immutable;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public class SuCaiFlowTaskManager<TTask>(ISuCaiFlowTaskStore<TTask> store) : ISuCaiFlowTaskManager
    where TTask : class {

    protected ISuCaiFlowTaskStore<TTask> Store { get; } = store ?? throw new ArgumentNullException(nameof(store));

    #region 自身实现
    public virtual ValueTask<long> CountAsync(CancellationToken cancellationToken = default) {
        return Store.CountAsync(cancellationToken);
    }

    public virtual ValueTask<long> CountAsync<TResult>(Func<IQueryable<TTask>, IQueryable<TResult>> query, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(query);

        return Store.CountAsync(query, cancellationToken);
    }

    public virtual async ValueTask<TTask> CreateAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(descriptor);

        var entity = await Store.InstantiateAsync(cancellationToken) ??
            throw new InvalidOperationException("无法创建实例");

        await PopulateAsync(entity, descriptor, cancellationToken);
        await CreateAsync(entity, cancellationToken);

        return entity;
    }

    public virtual async ValueTask CreateAsync(TTask task, CancellationToken cancellationToken = default) {
        await Store.CreateAsync(task, cancellationToken);
    }

    public virtual async ValueTask DeleteAsync(TTask task, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(task);

        await Store.DeleteAsync(task, cancellationToken);
    }

    public virtual async ValueTask<TTask?> FindByIdAsync(string identifier, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        var task = await Store.FindByIdAsync(identifier, cancellationToken);

        if (task is null) {
            return null;
        }

        return task;
    }

    public virtual ValueTask<TResult?> GetAsync<TResult>(Func<IQueryable<TTask>, IQueryable<TResult>> query, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(query);

        return GetAsync(static (tasks, query) => query(tasks), query, cancellationToken);
    }

    public virtual ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<TTask>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(query);

        return Store.GetAsync(query, state, cancellationToken);
    }

    public virtual IAsyncEnumerable<TTask> ListAsync(int? count = null, int? offset = null, CancellationToken cancellationToken = default) {
        return Store.ListAsync(count, offset, cancellationToken);
    }

    public virtual IAsyncEnumerable<TResult> ListAsync<TResult>(Func<IQueryable<TTask>, IQueryable<TResult>> query, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(query);

        return ListAsync(static (tasks, query) => query(tasks), query, cancellationToken);
    }

    public virtual IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<TTask>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(query);

        return Store.ListAsync(query, state, cancellationToken);
    }

    public virtual async ValueTask PopulateAsync(SuCaiFlowTaskDescriptor descriptor, TTask task, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(task);

        descriptor.TaskId = await Store.GetIdAsync(task, cancellationToken);
        descriptor.Name = await Store.GetNameAsync(task, cancellationToken);
        descriptor.SearchKeywords = await Store.GetSearchKeywordsAsync(task, cancellationToken);
        descriptor.StartUrl = await Store.GetStartUrlAsync(task, cancellationToken);
        descriptor.SiteIdentifier = await Store.GetSiteIdentifierAsync(task, cancellationToken);
        descriptor.Status = await Store.GetStatusAsync(task, cancellationToken);
        descriptor.ErrorMessage = await Store.GetErrorMessageAsync(task, cancellationToken);
        descriptor.CreatedAt = await Store.GetCreatedAtAsync(task, cancellationToken);
        descriptor.StartedAt = await Store.GetStartedAtAsync(task, cancellationToken);
        descriptor.CompletedAt = await Store.GetCompletedAtAsync(task, cancellationToken);
        descriptor.AssetsDownloadCount = await Store.GetAssetsDownloadCountAsync(task, cancellationToken);
        descriptor.AssetsCollectedCount = await Store.GetAssetsCollectedCountAsync(task, cancellationToken);
        descriptor.AssetsToCollectCount = await Store.GetAssetsToCollectCountAsync(task, cancellationToken);
        descriptor.KeepAfterCancel = await Store.GetKeepAfterCancelAsync(task, cancellationToken);

        descriptor.Parameters.Clear();
        foreach (var pair in await Store.GetParametersAsync(task, cancellationToken)) {
            descriptor.Parameters.Add(pair.Key, pair.Value);
        }
    }

    public virtual async ValueTask PopulateAsync(TTask task, SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(descriptor);

        await Store.SetNameAsync(task, descriptor.Name, cancellationToken);
        await Store.SetSearchKeywordsAsync(task, descriptor.SearchKeywords, cancellationToken);
        await Store.SetStartUrlAsync(task, descriptor.StartUrl, cancellationToken);
        await Store.SetSiteIdentifierAsync(task, descriptor.SiteIdentifier, cancellationToken);
        await Store.SetStatusAsync(task, descriptor.Status, cancellationToken);
        await Store.SetErrorMessageAsync(task, descriptor.ErrorMessage, cancellationToken);
        await Store.SetCreatedAtAsync(task, descriptor.CreatedAt, cancellationToken);
        await Store.SetStartedAtAsync(task, descriptor.StartedAt, cancellationToken);
        await Store.SetCompletedAtAsync(task, descriptor.CompletedAt, cancellationToken);
        await Store.SetAssetsDownloadCountAsync(task, descriptor.AssetsDownloadCount, cancellationToken);
        await Store.SetAssetsCollectedCountAsync(task, descriptor.AssetsCollectedCount, cancellationToken);
        await Store.SetAssetsToCollectCountAsync(task, descriptor.AssetsToCollectCount, cancellationToken);
        await Store.SetParametersAsync(task, descriptor.Parameters.ToImmutableDictionary(), cancellationToken);
        await Store.SetKeepAfterCancelAsync(task, descriptor.KeepAfterCancel, cancellationToken);
    }

    public virtual async ValueTask UpdateAsync(TTask task, SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(descriptor);

        await PopulateAsync(task, descriptor, cancellationToken);
        await UpdateAsync(task, cancellationToken);
    }

    public virtual async ValueTask UpdateAsync(TTask task, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(task);

        await Store.UpdateAsync(task, cancellationToken);
    }

    public virtual ValueTask ClearAssetsAsync(string identifier, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        return Store.ClearAssetsAsync(identifier, cancellationToken);
    }

    public virtual ValueTask<string?> GetIdAsync(TTask task, CancellationToken cancellationToken = default) {
        return Store.GetIdAsync(task, cancellationToken);
    }

    public virtual ValueTask<bool> GetKeepAfterCancelAsync(TTask task, CancellationToken cancellationToken = default) {
        return Store.GetKeepAfterCancelAsync(task, cancellationToken);
    }
    #endregion

    #region 接口实现
    ValueTask<TResult?> ISuCaiFlowTaskManager.GetAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken) where TResult : default {
        return GetAsync(query, cancellationToken);
    }

    ValueTask<TResult?> ISuCaiFlowTaskManager.GetAsync<TState, TResult>(Func<IQueryable<object>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) where TResult : default {
        return GetAsync(query, state, cancellationToken);
    }

    async ValueTask<object> ISuCaiFlowTaskManager.CreateAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken) {
        return await CreateAsync(descriptor, cancellationToken);
    }

    async ValueTask ISuCaiFlowTaskManager.CreateAsync(object task, CancellationToken cancellationToken) {
        await CreateAsync((TTask)task, cancellationToken);
    }

    ValueTask ISuCaiFlowTaskManager.DeleteAsync(object task, CancellationToken cancellationToken) {
        return DeleteAsync((TTask)task, cancellationToken);
    }

    async ValueTask<object?> ISuCaiFlowTaskManager.FindByIdAsync(string identifier, CancellationToken cancellationToken) {
        return await FindByIdAsync(identifier, cancellationToken);
    }

    ValueTask<long> ISuCaiFlowTaskManager.CountAsync(CancellationToken cancellationToken) {
        return CountAsync(cancellationToken);
    }

    ValueTask<long> ISuCaiFlowTaskManager.CountAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken) {
        return CountAsync(query, cancellationToken);
    }

    IAsyncEnumerable<object> ISuCaiFlowTaskManager.ListAsync(int? count, int? offset, CancellationToken cancellationToken) {
        return ListAsync(count, offset, cancellationToken);
    }

    IAsyncEnumerable<TResult> ISuCaiFlowTaskManager.ListAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken) {
        return ListAsync(query, cancellationToken);
    }

    IAsyncEnumerable<TResult> ISuCaiFlowTaskManager.ListAsync<TState, TResult>(Func<IQueryable<object>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) {
        return ListAsync(query, state, cancellationToken);
    }

    ValueTask ISuCaiFlowTaskManager.PopulateAsync(SuCaiFlowTaskDescriptor descriptor, object task, CancellationToken cancellationToken) {
        return PopulateAsync(descriptor, (TTask)task, cancellationToken);
    }

    ValueTask ISuCaiFlowTaskManager.PopulateAsync(object task, SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken) {
        return PopulateAsync((TTask)task, descriptor, cancellationToken);
    }

    ValueTask ISuCaiFlowTaskManager.UpdateAsync(object task, CancellationToken cancellationToken) {
        return UpdateAsync((TTask)task, cancellationToken);
    }

    ValueTask ISuCaiFlowTaskManager.UpdateAsync(object task, SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken) {
        return UpdateAsync((TTask)task, descriptor, cancellationToken);
    }

    ValueTask ISuCaiFlowTaskManager.ClearAssetsAsync(string identifier, CancellationToken cancellationToken) {
        return ClearAssetsAsync(identifier, cancellationToken);
    }

    ValueTask<string?> ISuCaiFlowTaskManager.GetIdAsync(object task, CancellationToken cancellationToken) {
        return GetIdAsync((TTask)task, cancellationToken);
    }

    ValueTask<bool> ISuCaiFlowTaskManager.GetKeepAfterCancelAsync(object task, CancellationToken cancellationToken) {
        return GetKeepAfterCancelAsync((TTask)task, cancellationToken);
    }
    #endregion
}
