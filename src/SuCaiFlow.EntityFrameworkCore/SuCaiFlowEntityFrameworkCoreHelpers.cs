using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore.Infrastructure;

using SuCaiFlow.EntityFrameworkCore;
using SuCaiFlow.EntityFrameworkCore.Models;

namespace Microsoft.EntityFrameworkCore;

public static class SuCaiFlowEntityFrameworkCoreHelpers {
    public static DbContextOptionsBuilder<TContext> UseSuCaiFlow<TContext>(this DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext {
        ((DbContextOptionsBuilder)builder).UseSuCaiFlow();
        return builder;
    }

    public static DbContextOptionsBuilder UseSuCaiFlow(this DbContextOptionsBuilder builder) {
        return builder.UseSuCaiFlow<SuCaiFlowEntityFrameworkCoreTask, SuCaiFlowEntityFrameworkCoreAsset, string>();
    }

    public static DbContextOptionsBuilder UseSuCaiFlow<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey>(this DbContextOptionsBuilder builder)
        where TKey : notnull, IEquatable<TKey> {
        return builder.UseSuCaiFlow<SuCaiFlowEntityFrameworkCoreTask<TKey>, SuCaiFlowEntityFrameworkCoreAsset<TKey>, TKey>();
    }

    public static DbContextOptionsBuilder UseSuCaiFlow<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TTask,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAsset,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey>(this DbContextOptionsBuilder builder)
        where TTask : SuCaiFlowEntityFrameworkCoreTask<TKey, TAsset>
        where TAsset : SuCaiFlowEntityFrameworkCoreAsset<TKey, TTask>
        where TKey : notnull, IEquatable<TKey> {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ReplaceService<IModelCustomizer, SuCaiFlowEntityFrameworkCoreCustomizer<TTask, TAsset, TKey>>();
    }

    public static ModelBuilder UseSuCaiFlow<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TTask,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAsset,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey>(this ModelBuilder builder)
        where TTask : SuCaiFlowEntityFrameworkCoreTask<TKey, TAsset>
        where TAsset : SuCaiFlowEntityFrameworkCoreAsset<TKey, TTask>
        where TKey : notnull, IEquatable<TKey> {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .ApplyConfiguration(new SuCaiFlowEntityFrameworkCoreTaskConfiguration<TTask, TAsset, TKey>())
            .ApplyConfiguration(new SuCaiFlowEntityFrameworkCoreAssetConfiguration<TAsset, TTask, TKey>());
    }

    internal static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> source, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(source);

        return ExecuteAsync(source, cancellationToken);

        static async IAsyncEnumerable<T> ExecuteAsync(IQueryable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken) {
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
                yield return element;
            }
        }
    }

    internal static string? Truncate(this string? value, int maxLength) {
        return string.IsNullOrEmpty(value) ? value : value.Length <= maxLength ? value : value[..maxLength];
    }
}
