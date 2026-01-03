using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SuCaiFlow.Contracts.Repositories;
using SuCaiFlow.Contracts.Services;

namespace SuCaiFlow.Core;

public class SuCaiFlowCoreBuilder(IServiceCollection services) {

    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    public SuCaiFlowCoreBuilder Configure(Action<SuCaiFlowCoreOptions> configuration) {
        ArgumentNullException.ThrowIfNull(configuration);

        Services.Configure(configuration);

        return this;
    }

    public SuCaiFlowCoreBuilder SetMaxConcurrency(int maxConcurrency)
        => Configure(options => options.MaxConcurrency = maxConcurrency);

    public SuCaiFlowCoreBuilder AddSiteCollector<TSiteCollector>()
        where TSiteCollector : ISiteCollector
        => Configure(options => options.SiteCollectorImplementTypes.Add(typeof(TSiteCollector)));

    public SuCaiFlowCoreBuilder ReplaceCollectedAssetRepository<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRepository>()
        where TRepository : ICollectedAssetRepository {
        Services.Replace(ServiceDescriptor.Scoped<ICollectedAssetRepository>(static provider =>
            provider.GetRequiredService<TRepository>()));
        return this;
    }

    public SuCaiFlowCoreBuilder ReplaceCollectionTaskConfigRepository<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRepository>()
        where TRepository : ICollectionTaskConfigRepository {
        Services.Replace(ServiceDescriptor.Scoped<ICollectionTaskConfigRepository>(static provider =>
            provider.GetRequiredService<TRepository>()));
        return this;
    }

    public SuCaiFlowCoreBuilder ReplaceCollectionTaskRepository<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRepository>()
        where TRepository : ICollectionTaskRepository {
        Services.Replace(ServiceDescriptor.Scoped<ICollectionTaskRepository>(static provider =>
            provider.GetRequiredService<TRepository>()));
        return this;
    }
}
