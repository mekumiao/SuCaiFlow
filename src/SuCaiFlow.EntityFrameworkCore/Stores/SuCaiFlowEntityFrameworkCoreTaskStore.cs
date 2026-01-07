using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using SuCaiFlow.Abstractions;
using SuCaiFlow.EntityFrameworkCore.Models;

using static SuCaiFlow.Abstractions.SuCaiFlowExceptions;

namespace SuCaiFlow.EntityFrameworkCore;

public class SuCaiFlowEntityFrameworkCoreTaskStore(ISuCaiFlowEntityFrameworkCoreContext context, IMemoryCache cache)
    : SuCaiFlowEntityFrameworkCoreTaskStore<SuCaiFlowEntityFrameworkCoreTask, SuCaiFlowEntityFrameworkCoreAsset, string>(context, cache) {
}

public class SuCaiFlowEntityFrameworkCoreTaskStore<TTask, TAsset, TKey>(ISuCaiFlowEntityFrameworkCoreContext context, IMemoryCache cache) : ISuCaiFlowTaskStore<TTask>
    where TTask : SuCaiFlowEntityFrameworkCoreTask<TKey, TAsset>
    where TAsset : SuCaiFlowEntityFrameworkCoreAsset<TKey, TTask>
    where TKey : notnull, IEquatable<TKey> {

    protected ISuCaiFlowEntityFrameworkCoreContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));
    protected IMemoryCache Cache { get; } = cache ?? throw new ArgumentNullException(nameof(cache));

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
        return await context.Set<TTask>().LongCountAsync(cancellationToken);
    }

    public virtual async ValueTask<long> CountAsync<TResult>(Func<IQueryable<TTask>, IQueryable<TResult>> query, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(query);

        var context = await Context.GetDbContextAsync(cancellationToken);
        return await query(context.Set<TTask>()).LongCountAsync(cancellationToken);
    }

    public virtual async ValueTask CreateAsync(TTask entity, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(entity);

        var context = await Context.GetDbContextAsync(cancellationToken);
        await context.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async ValueTask DeleteAsync(TTask entity, CancellationToken cancellationToken) {
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

    public virtual async ValueTask<TTask?> FindByIdAsync(string identifier, CancellationToken cancellationToken) {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        var context = await Context.GetDbContextAsync(cancellationToken);
        var key = ConvertIdentifierFromString(identifier);

        return GetTrackedEntity() is TTask task ? task : await QueryAsync();

        TTask? GetTrackedEntity() =>
            (from entry in context.ChangeTracker.Entries<TTask>()
             where entry.Entity.Id is TKey identifier && identifier.Equals(key)
             select entry.Entity).FirstOrDefault();

        Task<TTask?> QueryAsync() =>
            (from task in context.Set<TTask>().AsTracking()
             where task.Id!.Equals(key)
             select task).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual IAsyncEnumerable<TTask> FindByStatusAsync(string status, CancellationToken cancellationToken) {
        ArgumentException.ThrowIfNullOrEmpty(status);

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<TTask> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken) {
            var context = await Context.GetDbContextAsync(cancellationToken);

            var tasks = (from task in context.Set<TTask>().AsTracking()
                         where status.Equals(task.Status, StringComparison.Ordinal)
                         select task).AsAsyncEnumerable(cancellationToken);

            await foreach (var task in tasks) {
                yield return task;
            }
        }
    }

    public virtual ValueTask<int> GetAssetsCollectedCountAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        return new(task.AssetsCollectedCount);
    }

    public virtual ValueTask<int> GetAssetsDownloadCountAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        return new(task.AssetsDownloadCount);
    }

    public virtual async ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<TTask>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(query);

        var context = await Context.GetDbContextAsync(cancellationToken);
        return await query(context.Set<TTask>(), state).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual ValueTask<DateTimeOffset?> GetCompletedAtAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        if (task.CompletedAt is null) {
            return new(result: null);
        }

        return new(DateTime.SpecifyKind(task.CompletedAt.Value, DateTimeKind.Utc));
    }

    public virtual ValueTask<DateTimeOffset?> GetCreatedAtAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        if (task.CreatedAt is null) {
            return new(result: null);
        }

        return new(DateTime.SpecifyKind(task.CreatedAt.Value, DateTimeKind.Utc));
    }

    public virtual ValueTask<string?> GetErrorMessageAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        return new(task.ErrorMessage);
    }

    public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetParametersAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        if (string.IsNullOrEmpty(task.Parameters)) {
            return new(ImmutableDictionary.Create<string, JsonElement>());
        }

        // Note: parsing the stringified properties is an expensive operation.
        // To mitigate that, the resulting object is stored in the memory cache.
        var key = string.Concat("802d0c56-9f82-4384-8d81-9eb209987688", "\x1e", task.Parameters);
        var properties = Cache.GetOrCreate(key, entry => {
            entry.SetPriority(CacheItemPriority.High)
                 .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            using var document = JsonDocument.Parse(task.Parameters);
            var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();

            foreach (var property in document.RootElement.EnumerateObject()) {
                builder[property.Name] = property.Value.Clone();
            }

            return builder.ToImmutable();
        })!;

        return new(properties);
    }

    public ValueTask<string?> GetNameAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        return new(task.Name);
    }

    public virtual ValueTask<string?> GetSearchKeywordsAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        return new(task.SearchKeywords);
    }

    public virtual ValueTask<string?> GetSiteIdentifierAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        return new(task.SiteIdentifier);
    }

    public virtual ValueTask<DateTimeOffset?> GetStartedAtAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        if (task.StartedAt is null) {
            return new(result: null);
        }

        return new(DateTime.SpecifyKind(task.StartedAt.Value, DateTimeKind.Utc));
    }

    public virtual ValueTask<string?> GetStartUrlAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        return new(task.StartUrl);
    }

    public virtual ValueTask<string?> GetStatusAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        return new(task.Status);
    }

    public virtual ValueTask<string?> GetIdAsync(TTask entity, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(entity);

        return new(ConvertIdentifierToString(entity.Id));
    }

    public virtual ValueTask<int> GetAssetsToCollectCountAsync(TTask task, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        return new(task.AssetsToCollectCount);
    }

    public virtual ValueTask<TTask> InstantiateAsync(CancellationToken cancellationToken) {
        try {
            return new(Activator.CreateInstance<TTask>());
        }
        catch (MemberAccessException exception) {
            return new(Task.FromException<TTask>(new InvalidOperationException("无法创建对象", exception)));
        }
    }

    public virtual async IAsyncEnumerable<TTask> ListAsync(int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken) {
        var context = await Context.GetDbContextAsync(cancellationToken);

        var query = context.Set<TTask>().OrderBy(v => v.Id).AsTracking();

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

    public virtual IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<TTask>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(query);

        return ExecuteAsync(cancellationToken);

        async IAsyncEnumerable<TResult> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken) {
            var context = await Context.GetDbContextAsync(cancellationToken);

            await foreach (var application in query(context.Set<TTask>().AsTracking(), state).AsAsyncEnumerable(cancellationToken)) {
                yield return application;
            }
        }
    }

    public ValueTask SetNameAsync(TTask task, string? name, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.Name = name;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetAssetsCollectedCountAsync(TTask task, int count, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.AssetsCollectedCount = count;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetAssetsDownloadCountAsync(TTask task, int count, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.AssetsDownloadCount = count;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetCompletedAtAsync(TTask task, DateTimeOffset? date, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.CompletedAt = date?.UtcDateTime;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetCreatedAtAsync(TTask task, DateTimeOffset? date, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.CreatedAt = date?.UtcDateTime;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetErrorMessageAsync(TTask task, string? errorMessage, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.ErrorMessage = errorMessage;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetParametersAsync(TTask task, ImmutableDictionary<string, JsonElement> parameters, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        if (parameters is not { Count: > 0 }) {
            task.Parameters = null;

            return ValueTask.CompletedTask;
        }

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartObject();

        foreach (var item in parameters) {
            writer.WritePropertyName(item.Key);
            item.Value.WriteTo(writer);
        }

        writer.WriteEndObject();
        writer.Flush();

        task.Parameters = Encoding.UTF8.GetString(stream.ToArray());

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetSearchKeywordsAsync(TTask task, string? searchKeywords, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.SearchKeywords = searchKeywords;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetSiteIdentifierAsync(TTask task, string? siteIdentifier, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.SiteIdentifier = siteIdentifier;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetStartedAtAsync(TTask task, DateTimeOffset? date, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.StartedAt = date?.UtcDateTime;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetStartUrlAsync(TTask task, string? startUrl, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.StartUrl = startUrl;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetStatusAsync(TTask task, string? status, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.Status = status;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask SetAssetsToCollectCountAsync(TTask task, int count, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(task);

        task.AssetsToCollectCount = count;

        return ValueTask.CompletedTask;
    }

    public virtual async ValueTask UpdateAsync(TTask entity, CancellationToken cancellationToken) {
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
