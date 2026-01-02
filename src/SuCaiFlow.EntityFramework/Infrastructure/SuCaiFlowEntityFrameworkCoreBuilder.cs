using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SuCaiFlow.EntityFramework.Extensions;

namespace SuCaiFlow.EntityFramework.Infrastructure;

/// <summary>
/// Provides methods to configure SuCaiFlow Entity Framework Core integration.
/// </summary>
public class SuCaiFlowEntityFrameworkCoreBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuCaiFlowEntityFrameworkCoreBuilder"/> class.
    /// </summary>
    /// <param name="services">The services collection.</param>
    public SuCaiFlowEntityFrameworkCoreBuilder(IServiceCollection services)
        => Services = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Gets the services collection.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Configures SuCaiFlow to use the specified context as the database context.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context to use.</typeparam>
    /// <returns>The <see cref="SuCaiFlowEntityFrameworkCoreBuilder"/>.</returns>
    public SuCaiFlowEntityFrameworkCoreBuilder UseDbContext<TContext>()
        where TContext : DbContext
    {
        // Register the context as a service
        Services.AddDbContext<TContext>();

        // Add configuration to indicate which context type to use
        Services.Configure<SuCaiFlowEntityFrameworkCoreOptions>(options =>
        {
            options.ContextType = typeof(TContext);
        });

        // Register repositories
        Services.AddSuCaiFlowEntityFrameworkStores();

        return this;
    }
}
