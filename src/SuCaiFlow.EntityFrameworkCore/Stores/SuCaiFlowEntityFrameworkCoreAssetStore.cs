using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Abstractions;
using SuCaiFlow.EntityFrameworkCore.Models;

namespace SuCaiFlow.EntityFrameworkCore;

public class SuCaiFlowEntityFrameworkCoreAssetStore(ISuCaiFlowEntityFrameworkCoreContext context)
    : SuCaiFlowEntityFrameworkCoreAssetStore<SuCaiFlowEntityFrameworkCoreAsset, SuCaiFlowEntityFrameworkCoreTask, string>(context) {
}

public class SuCaiFlowEntityFrameworkCoreAssetStore<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAsset,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TTask,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey>(ISuCaiFlowEntityFrameworkCoreContext context)
    : ISuCaiFlowAssetStore<TAsset>
    where TAsset : SuCaiFlowEntityFrameworkCoreAsset<TKey, TTask>
    where TTask : SuCaiFlowEntityFrameworkCoreTask<TKey, TAsset>
    where TKey : notnull, IEquatable<TKey> {
    private readonly ISuCaiFlowEntityFrameworkCoreContext _context = context;

    public async ValueTask<long> CountAsync(CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        return await context.Set<TAsset>().LongCountAsync(cancellationToken);
    }

    public async ValueTask<long> CountAsync<TResult>(Func<IQueryable<TAsset>, IQueryable<TResult>> query, CancellationToken cancellationToken) {
        var context = await _context.GetDbContextAsync(cancellationToken);
        return await query(context.Set<TAsset>()).LongCountAsync(cancellationToken);
    }

    public ValueTask<TAsset> CreateAsync(TAsset entity, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask DeleteAsync(TAsset application, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask DeleteByTaskIdAsync(string identifier, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask<TAsset?> FindByAssetIdAsync(string identifier, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask<TAsset?> FindByIdAsync(string identifier, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TAsset> FindByStatusAsync(string status, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TAsset> FindByTaskIdAsync(string identifier, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<TAsset>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask<TAsset> InstantiateAsync(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TAsset> ListAsync(int? count, int? offset, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<TAsset>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public ValueTask UpdateAsync(TAsset application, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}
