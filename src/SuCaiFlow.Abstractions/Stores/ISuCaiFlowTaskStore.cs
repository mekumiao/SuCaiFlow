using System.Collections.Immutable;
using System.Text.Json;

namespace SuCaiFlow.Abstractions;

public interface ISuCaiFlowTaskStore<TTask> where TTask : class {
    ValueTask<long> CountAsync(CancellationToken cancellationToken);
    ValueTask<long> CountAsync<TResult>(Func<IQueryable<TTask>, IQueryable<TResult>> query, CancellationToken cancellationToken);

    ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<TTask>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken);
    IAsyncEnumerable<TTask> ListAsync(int? count, int? offset, CancellationToken cancellationToken);
    IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<TTask>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken);

    ValueTask<TTask> InstantiateAsync(CancellationToken cancellationToken);
    ValueTask CreateAsync(TTask entity, CancellationToken cancellationToken);
    ValueTask DeleteAsync(TTask entity, CancellationToken cancellationToken);
    ValueTask UpdateAsync(TTask entity, CancellationToken cancellationToken);
    ValueTask ClearAssetsAsync(string identifier, CancellationToken cancellationToken);

    ValueTask<TTask?> FindByIdAsync(string identifier, CancellationToken cancellationToken);
    IAsyncEnumerable<TTask> FindByStatusAsync(string status, CancellationToken cancellationToken);

    ValueTask SetNameAsync(TTask task, string? name, CancellationToken cancellationToken);
    ValueTask SetSearchKeywordsAsync(TTask task, string? searchKeywords, CancellationToken cancellationToken);
    ValueTask SetStartUrlAsync(TTask task, string? startUrl, CancellationToken cancellationToken);
    ValueTask SetSiteIdentifierAsync(TTask task, string? siteIdentifier, CancellationToken cancellationToken);
    ValueTask SetErrorMessageAsync(TTask task, string? errorMessage, CancellationToken cancellationToken);
    ValueTask SetStatusAsync(TTask task, string? status, CancellationToken cancellationToken);
    ValueTask SetCreatedAtAsync(TTask task, DateTimeOffset? date, CancellationToken cancellationToken);
    ValueTask SetStartedAtAsync(TTask task, DateTimeOffset? date, CancellationToken cancellationToken);
    ValueTask SetCompletedAtAsync(TTask task, DateTimeOffset? date, CancellationToken cancellationToken);
    ValueTask SetAssetsCollectedCountAsync(TTask task, int count, CancellationToken cancellationToken);
    ValueTask SetAssetsDownloadCountAsync(TTask task, int count, CancellationToken cancellationToken);
    ValueTask SetAssetsToCollectCountAsync(TTask task, int count, CancellationToken cancellationToken);
    ValueTask SetParametersAsync(TTask task, ImmutableDictionary<string, JsonElement> parameters, CancellationToken cancellationToken);

    ValueTask<string?> GetIdAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<string?> GetNameAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<string?> GetSearchKeywordsAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<string?> GetStartUrlAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<string?> GetSiteIdentifierAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<string?> GetErrorMessageAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<string?> GetStatusAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<DateTimeOffset?> GetCreatedAtAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<DateTimeOffset?> GetStartedAtAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<DateTimeOffset?> GetCompletedAtAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<int> GetAssetsCollectedCountAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<int> GetAssetsDownloadCountAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<int> GetAssetsToCollectCountAsync(TTask task, CancellationToken cancellationToken);
    ValueTask<ImmutableDictionary<string, JsonElement>> GetParametersAsync(TTask task, CancellationToken cancellationToken);
}
