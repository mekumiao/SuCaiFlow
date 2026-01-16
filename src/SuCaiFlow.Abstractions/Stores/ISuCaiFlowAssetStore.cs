namespace SuCaiFlow.Abstractions;

public interface ISuCaiFlowAssetStore<TAsset> where TAsset : class {
    ValueTask<long> CountAsync(CancellationToken cancellationToken);
    ValueTask<long> CountAsync<TResult>(Func<IQueryable<TAsset>, IQueryable<TResult>> query, CancellationToken cancellationToken);

    ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<TAsset>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken);
    IAsyncEnumerable<TAsset> ListAsync(int? count, int? offset, CancellationToken cancellationToken);
    IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<TAsset>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken);

    ValueTask<TAsset> InstantiateAsync(CancellationToken cancellationToken);
    ValueTask CreateAsync(TAsset entity, CancellationToken cancellationToken);
    ValueTask CreateRangeAsync(IEnumerable<TAsset> entities, CancellationToken cancellationToken);
    ValueTask UpdateAsync(TAsset application, CancellationToken cancellationToken);
    ValueTask DeleteAsync(TAsset application, CancellationToken cancellationToken);

    ValueTask<TAsset?> FindByIdAsync(string identifier, CancellationToken cancellationToken);
    IAsyncEnumerable<TAsset> FindByStatusAsync(string status, CancellationToken cancellationToken);

    ValueTask SetTaskIdAsync(TAsset asset, string? identifier, CancellationToken cancellationToken);
    ValueTask SetOrderNoAsync(TAsset asset, int no, CancellationToken cancellationToken);
    ValueTask SetNameAsync(TAsset asset, string? name, CancellationToken cancellationToken);
    ValueTask SetTitleAsync(TAsset asset, string? title, CancellationToken cancellationToken);
    ValueTask SetDescriptionAsync(TAsset asset, string? description, CancellationToken cancellationToken);
    ValueTask SetOriginalUrlAsync(TAsset asset, string? originalUrl, CancellationToken cancellationToken);
    ValueTask SetLandingUrlAsync(TAsset asset, string? landingUrl, CancellationToken cancellationToken);
    ValueTask SetObjectKeyAsync(TAsset asset, string? objectKey, CancellationToken cancellationToken);
    ValueTask SetContentTypeAsync(TAsset asset, string? contentType, CancellationToken cancellationToken);
    ValueTask SetStatusAsync(TAsset asset, string? status, CancellationToken cancellationToken);
    ValueTask SetSizeAsync(TAsset asset, long size, CancellationToken cancellationToken);
    ValueTask SetCreatedAtAsync(TAsset asset, DateTimeOffset? date, CancellationToken cancellationToken);
    ValueTask SetDownloadedAtAsync(TAsset asset, DateTimeOffset? date, CancellationToken cancellationToken);

    ValueTask<string?> GetIdAsync(TAsset token, CancellationToken cancellationToken);
    ValueTask<string?> GetTaskIdAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<int> GetOrderNoAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<string?> GetNameAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<string?> GetTitleAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<string?> GetDescriptionAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<string?> GetOriginalUrlAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<string?> GetLandingUrlAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<string?> GetObjectKeyAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<string?> GetContentTypeAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<string?> GetStatusAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<long> GetSizeAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<DateTimeOffset?> GetCreatedAtAsync(TAsset asset, CancellationToken cancellationToken);
    ValueTask<DateTimeOffset?> GetDownloadedAtAsync(TAsset asset, CancellationToken cancellationToken);
}
