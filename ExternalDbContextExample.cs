using Microsoft.EntityFrameworkCore;
using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.EntityFramework.Extensions;

// 示例：如何在外部项目中使用外部DbContext与SuCaiFlow集成
public class ExternalDbContext : DbContext
{
    public ExternalDbContext(DbContextOptions<ExternalDbContext> options) : base(options)
    {
    }

    public DbSet<CollectionTask> CollectionTasks { get; set; }
    public DbSet<CollectedAsset> CollectedAssets { get; set; }
    public DbSet<CollectionTaskConfig> CollectionTaskConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 应用SuCaiFlow的实体配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SuCaiFlowDbContext).Assembly);

        // 可以在这里添加其他实体的配置
        base.OnModelCreating(modelBuilder);
    }
}

// 在Program.cs或Startup.cs中的配置示例
public class ExternalConfigurationExample
{
    public void ConfigureServices(IServiceCollection services)
    {
        // 注册SuCaiFlow核心服务
        services.AddSuCaiFlow();

        // 注册SuCaiFlow EF Core服务
        services.AddSuCaiFlowEntityFrameworkCore()
            .UseEntityFrameworkCore()
            .UseDbContext<ExternalDbContext>(); // 使用外部DbContext

        // 配置外部DbContext
        services.AddDbContext<ExternalDbContext>(options =>
            options.UseSqlServer("connection_string_here")
                   .UseSuCaiFlow()); // 应用SuCaiFlow特定配置
    }
}
