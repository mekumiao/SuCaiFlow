using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SuCaiFlow.Contracts.Interfaces;
using SuCaiFlow.EntityFramework.Data;
using SuCaiFlow.EntityFramework.Infrastructure;

namespace SuCaiFlow.EntityFramework.Extensions;

public static class EntityFrameworkExtensions {
    public static IServiceCollection AddSuCaiFlowDbContext<TContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        where TContext : SuCaiFlowDbContext {
        services.AddDbContext<TContext>(optionsAction);
        return services;
    }

    public static IServiceCollection AddSuCaiFlowDbContext<TContext>(this IServiceCollection services, string connectionString)
        where TContext : SuCaiFlowDbContext {
        // This method is deprecated. Use AddDbContext directly with your chosen database provider.
        throw new NotSupportedException("This method is deprecated. Use AddDbContext directly with your chosen database provider.");
        // Example: services.AddDbContext<TContext>(options => options.UseNpgsql(connectionString));
    }

    /// <summary>
    /// Registers the SuCaiFlow repositories and configures the DbContext.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <returns>The services collection.</returns>
    public static IServiceCollection AddSuCaiFlowEntityFrameworkStores(this IServiceCollection services)
    {
        services.AddSuCaiFlowRepositories();
        return services;
    }

    public static IServiceCollection AddSuCaiFlowRepositories(this IServiceCollection services) {
        services.TryAddScoped<ICollectionTaskRepository, CollectionTaskRepository>();
        services.TryAddScoped<IAssetRepository, AssetRepository>();
        services.TryAddScoped<ICollectionTaskConfigRepository, CollectionTaskConfigRepository>();
        return services;
    }

    /// <summary>
    /// Configures SuCaiFlow to use Entity Framework Core as the backing store.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <returns>A builder that allows configuring SuCaiFlow services.</returns>
    public static IServiceCollection AddSuCaiFlowEntityFrameworkCore(this IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    /// Configures SuCaiFlow to use Entity Framework Core as the backing store.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <returns>A builder that allows configuring Entity Framework Core integration.</returns>
    public static SuCaiFlowEntityFrameworkCoreBuilder UseEntityFrameworkCore(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register the Entity Framework Core options
        services.AddOptions<SuCaiFlowEntityFrameworkCoreOptions>();

        return new SuCaiFlowEntityFrameworkCoreBuilder(services);
    }

    /// <summary>
    /// Configures Entity Framework Core to use SuCaiFlow entities.
    /// </summary>
    /// <param name="options">The options builder.</param>
    /// <returns>The options builder.</returns>
    public static DbContextOptionsBuilder UseSuCaiFlow(this DbContextOptionsBuilder options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        // This method can be used to configure EF Core options specific to SuCaiFlow
        // For now, it just returns the options builder
        return options;
    }
}
