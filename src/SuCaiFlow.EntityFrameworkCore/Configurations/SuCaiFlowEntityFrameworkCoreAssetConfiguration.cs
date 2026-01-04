using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SuCaiFlow.EntityFrameworkCore.Models;

namespace SuCaiFlow.EntityFrameworkCore;

[EditorBrowsable(EditorBrowsableState.Never)]
public class SuCaiFlowEntityFrameworkCoreAssetConfiguration<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAsset,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TTask,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey>
    : IEntityTypeConfiguration<TAsset>
    where TAsset : SuCaiFlowEntityFrameworkCoreAsset<TKey, TTask>
    where TTask : SuCaiFlowEntityFrameworkCoreTask<TKey, TAsset>
    where TKey : notnull, IEquatable<TKey> {
    public void Configure(EntityTypeBuilder<TAsset> builder) {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(500);
        builder.Property(e => e.Title).HasMaxLength(500);
        builder.Property(e => e.Status).HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.OriginalUri).HasMaxLength(2000);
        builder.Property(e => e.StorageName).HasMaxLength(500);
        builder.Property(e => e.ContentType).HasMaxLength(100);
        builder.Property(e => e.ErrorMessage).HasMaxLength(1000);

        builder.Property(e => e.ConcurrencyToken)
               .HasMaxLength(50)
               .IsConcurrencyToken();

        builder.ToTable("SuCaiFlowAssets");
    }
}
