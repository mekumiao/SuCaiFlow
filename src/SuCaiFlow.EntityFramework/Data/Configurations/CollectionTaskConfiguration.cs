using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.EntityFramework.Data.Configurations;

public class CollectionTaskConfiguration : IEntityTypeConfiguration<CollectionTask>
{
    public void Configure(EntityTypeBuilder<CollectionTask> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Url).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Selector).HasMaxLength(500);
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.Property(e => e.Parameters).HasConversion<DictionaryJsonConverter<string, string>>();

        builder.HasMany(e => e.Assets)
              .WithOne(a => a.CollectionTask)
              .HasForeignKey(a => a.CollectionTaskId)
              .OnDelete(DeleteBehavior.Cascade);

        // 配置 CollectionTask 和 CollectionTaskConfig 的关系
        builder.HasOne(e => e.Config)
              .WithMany()
              .HasForeignKey(e => e.ConfigId)
              .OnDelete(DeleteBehavior.SetNull);
    }
}
