using Microsoft.EntityFrameworkCore;

using SuCaiFlow.Contracts.Entities;

namespace SuCaiFlow.EntityFramework.Data;

public class SuCaiFlowDbContext(DbContextOptions<SuCaiFlowDbContext> options) : DbContext(options) {
    public DbSet<CollectionTask> CollectionTasks { get; set; }
    public DbSet<CollectedAsset> CollectedAssets { get; set; }
    public DbSet<CollectionTaskConfig> CollectionTaskConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // 应用所有实体配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SuCaiFlowDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
