using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.EntityFramework.Data.Configurations;

public class CollectedAssetConfiguration : IEntityTypeConfiguration<CollectedAsset>
{
    public void Configure(EntityTypeBuilder<CollectedAsset> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Url).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.LocalPath).HasMaxLength(500);
        builder.Property(e => e.ContentType).HasMaxLength(100);
    }
}
