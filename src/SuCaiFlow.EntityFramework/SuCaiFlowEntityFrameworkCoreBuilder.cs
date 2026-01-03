using System.ComponentModel;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SuCaiFlow.EntityFrameworkCore;

public class SuCaiFlowEntityFrameworkCoreBuilder(IServiceCollection services) {

    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    public SuCaiFlowEntityFrameworkCoreBuilder Configure(Action<SuCaiFlowEntityFrameworkCoreOptions> configuration) {
        ArgumentNullException.ThrowIfNull(configuration);

        Services.Configure(configuration);

        return this;
    }

    //public SuCaiFlowEntityFrameworkCoreBuilder ReplaceDefaultEntities<
    //    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCollectionTask,
    //    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCollectedAsset,
    //    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCollectionTask>() {
    //    Services.Replace(ServiceDescriptor.Scoped<ICollectionTaskService>(static provider =>
    //        provider.GetRequiredService<CollectionTaskService>()));
    //    Services.Replace(ServiceDescriptor.Scoped<ICollectedAssetService>(static provider =>
    //        provider.GetRequiredService<CollectedAssetService>()));
    //    return this;
    //}

    public SuCaiFlowEntityFrameworkCoreBuilder UseDbContext<TContext>()
        where TContext : DbContext {

        Services.Replace(ServiceDescriptor.Scoped<
            ISuCaiFlowEntityFrameworkCoreContext, SuCaiFlowEntityFrameworkCoreContext<TContext>>());

        return this;
    }
}
