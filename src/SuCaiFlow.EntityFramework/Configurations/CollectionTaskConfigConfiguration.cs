using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.EntityFrameworkCore.Configurations;

public class CollectionTaskConfigConfiguration : IEntityTypeConfiguration<CollectionTaskConfig>
{
    public void Configure(EntityTypeBuilder<CollectionTaskConfig> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.SiteUrl).HasMaxLength(2000);
        builder.Property(e => e.Selector).HasMaxLength(500);
        builder.Property(e => e.PaginationSelector).HasMaxLength(500);
        builder.Property(e => e.NextPagePattern).HasMaxLength(500);
        builder.Property(e => e.RequestHeaders).HasConversion<DictionaryJsonConverter<string, string>>();
        builder.Property(e => e.SearchParameters).HasConversion<DictionaryJsonConverter<string, string>>();
        builder.Property(e => e.AdditionalSettings).HasConversion<DictionaryJsonConverter<string, string>>();
    }
}
