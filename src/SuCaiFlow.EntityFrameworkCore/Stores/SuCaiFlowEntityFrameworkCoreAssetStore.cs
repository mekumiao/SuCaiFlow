using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Abstractions;
using SuCaiFlow.EntityFrameworkCore.Models;

using static SuCaiFlow.Abstractions.SuCaiFlowExceptions;

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

    protected ISuCaiFlowEntityFrameworkCoreContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));

    public virtual TKey? ConvertIdentifierFromString(string? identifier) {
        if (string.IsNullOrEmpty(identifier)) {
            return default;
        }

        if (typeof(TKey) == typeof(string)) {
            return (TKey?)(object?)identifier;
        }

        else {
            var converter =
#if SUPPORTS_TYPE_DESCRIPTOR_TYPE_REGISTRATION
                TypeDescriptor.GetConverterFromRegisteredType(typeof(TKey));
#else
                TypeDescriptor.GetConverter(typeof(TKey));
#endif

            return (TKey?)converter.ConvertFromInvariantString(identifier);
        }
    }

    public virtual async ValueTask<long> CountAsync(CancellationToken cancellationToken) {
        var context = await Context.GetDbContextAsync(cancellationToken);
        return await context.Set<TAsset>().LongCountAsync(cancellationToken);
    }

    public virtual async ValueTask<long> CountAsync<TResult>(Func<IQueryable<TAsset>, IQueryable<TResult>> query, CancellationToken cancellationToken) {
        var context = await Context.GetDbContextAsync(cancellationToken);
        return await query(context.Set<TAsset>()).LongCountAsync(cancellationToken);
    }

    public virtual async ValueTask CreateAsync(TAsset entity, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(entity);

        var context = await Context.GetDbContextAsync(cancellationToken);
        await context.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async ValueTask CreateRangeAsync(ICollection<TAsset> entities, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count != 0) {
            var context = await Context.GetDbContextAsync(cancellationToken);
            await context.AddRangeAsync(entities, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async ValueTask DeleteAsync(TAsset entity, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(entity);

        var context = await Context.GetDbContextAsync(cancellationToken);

        context.Remove(entity);

        try {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex) {
            context.Entry(entity).State = EntityState.Unchanged;
            throw new ConcurrencyException("数据发生变化", ex);
        }
    }

    public virtual async ValueTask<TAsset?> FindByIdAsync(string identifier, CancellationToken cancellationToken) {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        var context = await Context.GetDbContextAsync(cancellationToken);
        var key = ConvertIdentifierFromString(identifier);

        return GetTrackedEntity() is TAsset task ? task : await QueryAsync();

        TAsset? GetTrackedEntity() =>
            (from entry in context.ChangeTracker.Entries<TAsset>()
             where entry.Entity.Id is TKey identifier && identifier.Equals(key)
             select entry.Entity).FirstOrDefault();

        Task<TAsset?> QueryAsync() =>
            (from asset in context.Set<TAsset>().AsTracking()
             where asset.Id!.Equals(key)
             select asset).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual IAsyncEnumerable<TAsset> FindByStatusAsync(string status, CancellationToken cancellationToken) {
        ArgumentException.ThrowIfNullOrEmpty(status);

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<TAsset> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken) {
            var context = await Context.GetDbContextAsync(cancellationToken);

            var tasks = (from task in context.Set<TAsset>().AsTracking()
                         where status.Equals(task.Status, StringComparison.Ordinal)
                         select task).AsAsyncEnumerable(cancellationToken);

            await foreach (var task in tasks) {
                yield return task;
            }
        }
    }

    public virtual async ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<TAsset>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(query);

        var context = await Context.GetDbContextAsync(cancellationToken);
        return await query(context.Set<TAsset>(), state).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual ValueTask<string?> GetIdAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(ConvertIdentifierToString(asset.Id));
    }

    public virtual ValueTask<string?> GetContentTypeAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.ContentType);
    }

    public virtual ValueTask<string?> GetDescriptionAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.Description);
    }

    public virtual ValueTask<DateTimeOffset?> GetCreatedAtAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.CreatedAt);
    }

    public virtual ValueTask<DateTimeOffset?> GetDownloadedAtAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.DownloadedAt);
    }

    public virtual ValueTask<string?> GetLandingUrlAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.LandingUrl);
    }

    public virtual ValueTask<string?> GetNameAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.Name);
    }

    public virtual ValueTask<string?> GetOriginalUrlAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.OriginalUrl);
    }

    public virtual ValueTask<long> GetSizeAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.Size);
    }

    public virtual ValueTask<string?> GetStatusAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.Status);
    }

    public virtual ValueTask<string?> GetObjectKeyAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.ObjectKey);
    }

    public virtual async ValueTask<string?> GetTaskIdAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        var context = await Context.GetDbContextAsync(cancellationToken);

        // If the flowTask is not attached to the token, try to load it manually.
        if (asset.Task is null) {
            var reference = context.Entry(asset).Reference(entry => entry.Task);
            if (reference.EntityEntry.State is EntityState.Detached) {
                return null;
            }

            await reference.LoadAsync(cancellationToken);
        }

        if (asset.Task is null) {
            return null;
        }

        return ConvertIdentifierToString(asset.Task.Id);
    }

    public virtual ValueTask<string?> GetTitleAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.Title);
    }

    public ValueTask<int> GetOrderNoAsync(TAsset asset, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        return new(asset.OrderNo);
    }

    public virtual ValueTask<TAsset> InstantiateAsync(CancellationToken cancellationToken) {
        try {
            return new(Activator.CreateInstance<TAsset>());
        }
        catch (MemberAccessException exception) {
            return new(Task.FromException<TAsset>(new InvalidOperationException("无法创建对象", exception)));
        }
    }

    public virtual async IAsyncEnumerable<TAsset> ListAsync(int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken) {
        var context = await Context.GetDbContextAsync(cancellationToken);

        var query = context.Set<TAsset>().OrderBy(v => v.Id).AsTracking();

        if (offset.HasValue) {
            query = query.Skip(offset.Value);
        }

        if (count.HasValue) {
            query = query.Take(count.Value);
        }

        await foreach (var item in query.AsAsyncEnumerable(cancellationToken)) {
            yield return item;
        }
    }

    public virtual IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<TAsset>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(query);

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<TResult> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken) {
            var context = await Context.GetDbContextAsync(cancellationToken);

            await foreach (var application in query(context.Set<TAsset>().AsTracking(), state).AsAsyncEnumerable(cancellationToken)) {
                yield return application;
            }
        }
    }

    public virtual ValueTask SetContentTypeAsync(TAsset asset, string? contentType, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.ContentType = contentType;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetDescriptionAsync(TAsset asset, string? description, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.Description = description.Truncate(5000);

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetCreatedAtAsync(TAsset asset, DateTimeOffset? date, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.CreatedAt = date?.UtcDateTime;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetDownloadedAtAsync(TAsset asset, DateTimeOffset? date, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.DownloadedAt = date?.UtcDateTime;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetLandingUrlAsync(TAsset asset, string? landingUrl, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.LandingUrl = landingUrl.Truncate(2000);

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetNameAsync(TAsset asset, string? name, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.Name = name.Truncate(500);

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetOriginalUrlAsync(TAsset asset, string? originalUrl, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.OriginalUrl = originalUrl.Truncate(2000);

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetSizeAsync(TAsset asset, long size, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.Size = size;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetStatusAsync(TAsset asset, string? status, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.Status = status;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetObjectKeyAsync(TAsset asset, string? objectKey, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.ObjectKey = objectKey;

        return ValueTask.CompletedTask;
    }

    public virtual async ValueTask SetTaskIdAsync(TAsset asset, string? identifier, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        var context = await Context.GetDbContextAsync(cancellationToken);

        if (!string.IsNullOrEmpty(identifier)) {
            asset.Task = await context.Set<TTask>()
                .FindAsync([ConvertIdentifierFromString(identifier)], cancellationToken);
        }
        else {
            // If the flowTask is not attached to the token, try to load it manually.
            if (asset.Task is null) {
                var reference = context.Entry(asset).Reference(entry => entry.Task);
                if (reference.EntityEntry.State is EntityState.Detached) {
                    return;
                }

                await reference.LoadAsync(cancellationToken);
            }

            asset.Task = null;
        }
    }

    public virtual ValueTask SetTitleAsync(TAsset asset, string? title, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.Title = title.Truncate(2000);

        return ValueTask.CompletedTask;
    }

    public ValueTask SetOrderNoAsync(TAsset asset, int no, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(asset);

        asset.OrderNo = no;

        return ValueTask.CompletedTask;
    }

    public virtual async ValueTask UpdateAsync(TAsset entity, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(entity);

        var context = await Context.GetDbContextAsync(cancellationToken);

        context.Attach(entity);

        // Generate a new concurrency token and attach it
        // to the application before persisting the changes.
        entity.ConcurrencyToken = Guid.NewGuid().ToString();

        context.Update(entity);

        try {
            await context.SaveChangesAsync(cancellationToken);
        }

        catch (DbUpdateConcurrencyException exception) {
            // Reset the state of the entity to prevents future calls to SaveChangesAsync() from failing.
            context.Entry(entity).State = EntityState.Unchanged;

            throw new ConcurrencyException("数据发生变化", exception);
        }
    }

    public virtual string? ConvertIdentifierToString(TKey? identifier) {
        if (Equals(identifier, default(TKey))) {
            return null;
        }

        // Optimization: if the key is a string, directly return it as-is.
        if (identifier is string value) {
            return value;
        }

        else {
            var converter =
#if SUPPORTS_TYPE_DESCRIPTOR_TYPE_REGISTRATION
                TypeDescriptor.GetConverterFromRegisteredType(typeof(TKey));
#else
                TypeDescriptor.GetConverter(typeof(TKey));
#endif

            return converter.ConvertToInvariantString(identifier);
        }
    }
}
