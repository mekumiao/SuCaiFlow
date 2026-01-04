using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using SuCaiFlow.EntityFrameworkCore.Models;

namespace SuCaiFlow.EntityFrameworkCore;

public sealed class SuCaiFlowEntityFrameworkCoreCustomizer<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TTask,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAsset,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey>(ModelCustomizerDependencies dependencies)
    : RelationalModelCustomizer(dependencies)
    where TTask : SuCaiFlowEntityFrameworkCoreTask<TKey, TAsset>
    where TAsset : SuCaiFlowEntityFrameworkCoreAsset<TKey, TTask>
    where TKey : notnull, IEquatable<TKey> {
    public override void Customize(ModelBuilder modelBuilder, DbContext context) {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(context);

        modelBuilder.UseSuCaiFlow<TTask, TAsset, TKey>();

        base.Customize(modelBuilder, context);
    }
}
