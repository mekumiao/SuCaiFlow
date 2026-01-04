using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Abstractions;
using SuCaiFlow.EntityFrameworkCore.Models;

namespace SuCaiFlow.EntityFrameworkCore;

public class SuCaiFlowEntityFrameworkCoreTaskStore(ISuCaiFlowEntityFrameworkCoreContext context)
    : SuCaiFlowEntityFrameworkCoreTaskStore<SuCaiFlowEntityFrameworkCoreTask, SuCaiFlowEntityFrameworkCoreAsset, string>(context) {
}

public class SuCaiFlowEntityFrameworkCoreTaskStore<TTask, TAsset, TKey>(ISuCaiFlowEntityFrameworkCoreContext context)
    : ISuCaiFlowTaskStore<TTask>
    where TTask : SuCaiFlowEntityFrameworkCoreTask<TKey, TAsset>
    where TAsset : SuCaiFlowEntityFrameworkCoreAsset<TKey, TTask>
    where TKey : notnull, IEquatable<TKey> {
    private readonly ISuCaiFlowEntityFrameworkCoreContext _context = context;

    public async ValueTask<long> CountAsync(CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        return await context.Set<TTask>().LongCountAsync(cancellationToken);
    }

    public async ValueTask<long> CountAsync<TResult>(Func<IQueryable<TTask>, IQueryable<TResult>> query, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        return await query(context.Set<TTask>()).LongCountAsync(cancellationToken);
    }

    public ValueTask<TTask> CreateAsync(TTask entity, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask DeleteAsync(TTask application, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask DeleteByTaskIdAsync(string identifier, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask<TTask?> FindByAssetIdAsync(string identifier, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask<TTask?> FindByIdAsync(string identifier, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TTask> FindByStatusAsync(string status, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TTask> FindByTaskIdAsync(string identifier, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<TTask>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask<TTask> InstantiateAsync(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TTask> ListAsync(int? count, int? offset, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<TTask>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask UpdateAsync(TTask application, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}
