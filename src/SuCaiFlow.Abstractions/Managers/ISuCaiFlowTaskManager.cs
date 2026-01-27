namespace SuCaiFlow.Abstractions;

public interface ISuCaiFlowTaskManager {
    ValueTask<long> CountAsync(CancellationToken cancellationToken = default);
    ValueTask<long> CountAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken = default);

    ValueTask<TResult?> GetAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken = default);
    ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<object>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken = default);
    IAsyncEnumerable<object> ListAsync(int? count = null, int? offset = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TResult> ListAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<object>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken = default);
    ValueTask<object?> FindByIdAsync(string identifier, CancellationToken cancellationToken = default);

    ValueTask<object> CreateAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default);
    ValueTask CreateAsync(object task, CancellationToken cancellationToken = default);
    ValueTask DeleteAsync(object task, CancellationToken cancellationToken = default);
    ValueTask UpdateAsync(object task, CancellationToken cancellationToken = default);
    ValueTask UpdateAsync(object task, SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default);
    ValueTask PopulateAsync(SuCaiFlowTaskDescriptor descriptor, object task, CancellationToken cancellationToken = default);
    ValueTask PopulateAsync(object task, SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default);
    ValueTask ClearAssetsAsync(string identifier, CancellationToken cancellationToken = default);

    ValueTask<string?> GetIdAsync(object task, CancellationToken cancellationToken = default);
}
