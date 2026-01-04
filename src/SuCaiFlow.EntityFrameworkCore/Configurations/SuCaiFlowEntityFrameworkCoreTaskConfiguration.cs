using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SuCaiFlow.EntityFrameworkCore.Models;

namespace SuCaiFlow.EntityFrameworkCore;

public class SuCaiFlowEntityFrameworkCoreTaskConfiguration<TTask, TAsset, Tkey>
    : IEntityTypeConfiguration<TTask>
    where TTask : SuCaiFlowEntityFrameworkCoreTask<Tkey, TAsset>
    where TAsset : SuCaiFlowEntityFrameworkCoreAsset<Tkey, TTask>
    where Tkey : notnull, IEquatable<Tkey> {
    public void Configure(EntityTypeBuilder<TTask> builder) {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DisplayName).HasMaxLength(500);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.StartUri).HasMaxLength(2000);
        builder.Property(e => e.Status).HasMaxLength(50);
        builder.Property(e => e.ErrorMessage).HasMaxLength(1000);

        builder.Property(e => e.ConcurrencyToken)
               .HasMaxLength(50)
               .IsConcurrencyToken();

        builder.HasMany(e => e.Assets)
               .WithOne(a => a.Task)
               .HasForeignKey(a => a.TaskId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("SuCaiFlowTasks");
    }
}
