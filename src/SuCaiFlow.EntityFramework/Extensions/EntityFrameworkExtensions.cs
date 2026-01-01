using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SuCaiFlow.Contracts.Interfaces;
using SuCaiFlow.EntityFramework.Data;

namespace SuCaiFlow.EntityFramework.Extensions;

public static class EntityFrameworkExtensions {
    public static IServiceCollection AddSuCaiFlowDbContext<TContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        where TContext : SuCaiFlowDbContext {
        services.AddDbContext<TContext>(optionsAction);
        return services;
    }

    public static IServiceCollection AddSuCaiFlowDbContext<TContext>(this IServiceCollection services, string connectionString)
        where TContext : SuCaiFlowDbContext {
        services.AddDbContext<TContext>(options => options.UseNpgsql(connectionString));
        return services;
    }

    public static IServiceCollection AddSuCaiFlowRepositories(this IServiceCollection services) {
        services.AddScoped<ICollectionTaskRepository, CollectionTaskRepository>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<ICollectionTaskConfigRepository, CollectionTaskConfigRepository>();
        return services;
    }
}
