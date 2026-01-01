using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.EntityFramework.Data;

public class SuCaiFlowDbContext(DbContextOptions<SuCaiFlowDbContext> options) : DbContext(options) {
    public DbSet<CollectionTask> CollectionTasks { get; set; }
    public DbSet<CollectedAsset> CollectedAssets { get; set; }
    public DbSet<CollectionTaskConfig> CollectionTaskConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // 为Dictionary<string, string>类型添加转换器
        var dictionaryConverter = new DictionaryJsonConverter<string, string>();

        // 配置 CollectionTask 实体
        modelBuilder.Entity<CollectionTask>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Selector).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.Parameters).HasConversion(dictionaryConverter);

            entity.HasMany(e => e.Assets)
                  .WithOne(a => a.CollectionTask)
                  .HasForeignKey(a => a.CollectionTaskId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置 CollectionTaskConfig 实体
        modelBuilder.Entity<CollectionTaskConfig>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.SiteUrl).HasMaxLength(2000);
            entity.Property(e => e.Selector).HasMaxLength(500);
            entity.Property(e => e.PaginationSelector).HasMaxLength(500);
            entity.Property(e => e.NextPagePattern).HasMaxLength(500);
            entity.Property(e => e.RequestHeaders).HasConversion(dictionaryConverter);
            entity.Property(e => e.SearchParameters).HasConversion(dictionaryConverter);
            entity.Property(e => e.AdditionalSettings).HasConversion(dictionaryConverter);
        });

        // 配置 CollectionTask 和 CollectionTaskConfig 的关系
        modelBuilder.Entity<CollectionTask>(entity => {
            entity.HasOne<CollectionTaskConfig>(e => e.Config)
                  .WithMany()
                  .HasForeignKey(e => e.ConfigId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 配置 CollectedAsset 实体
        modelBuilder.Entity<CollectedAsset>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.LocalPath).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(100);
        });

        base.OnModelCreating(modelBuilder);
    }
}

public class DictionaryJsonConverter<TKey, TValue> : ValueConverter<Dictionary<TKey, TValue>, string>
    where TKey : notnull {
    public DictionaryJsonConverter() : base(
        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
        v => string.IsNullOrEmpty(v)
            ? new Dictionary<TKey, TValue>()
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<TKey, TValue>()) {
    }
}
