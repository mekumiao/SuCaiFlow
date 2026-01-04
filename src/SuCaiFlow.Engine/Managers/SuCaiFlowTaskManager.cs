using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public class SuCaiFlowTaskManager<TTask>(ISuCaiFlowTaskStore<TTask> taskStore) : ISuCaiFlowTaskManager
    where TTask : class {
    private readonly ISuCaiFlowTaskStore<TTask> _taskStore = taskStore;

    public ValueTask<long> CountAsync(CancellationToken cancellationToken = default) {
        return _taskStore.CountAsync(cancellationToken);
    }

    public ValueTask<long> CountAsync<TResult>(Func<IQueryable<object>, IQueryable<TResult>> query, CancellationToken cancellationToken = default) {
        return _taskStore.CountAsync(query, cancellationToken);
    }

    public ValueTask<object> CreateAsync(SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask CreateAsync(object task, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask DeleteAsync(object task, CancellationToken cancellationToken = default) {
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

    public ValueTask PopulateAsync(SuCaiFlowTaskDescriptor descriptor, object task, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask PopulateAsync(object task, SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask UpdateAsync(object task, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public ValueTask UpdateAsync(object task, SuCaiFlowTaskDescriptor descriptor, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }
}
