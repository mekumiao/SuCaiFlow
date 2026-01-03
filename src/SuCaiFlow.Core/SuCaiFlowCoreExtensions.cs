using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using SuCaiFlow.Contracts;
using SuCaiFlow.Contracts.Services;
using SuCaiFlow.Core.Services;

namespace SuCaiFlow.Core;

public static class SuCaiFlowCoreExtensions {
    public static SuCaiFlowCoreBuilder AddCore(this SuCaiFlowBuilder builder) {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddLogging();
        builder.Services.AddMemoryCache();
        builder.Services.AddOptions();

        builder.Services.TryAddScoped<ICollectedAssetService, CollectedAssetService>();
        builder.Services.TryAddScoped<ICollectionTaskService, CollectionTaskService>();
        builder.Services.TryAddScoped<ITaskExecutionService, TaskExecutionService>();
        builder.Services.TryAddScoped<IEventPublisher, EventPublisher>();
        builder.Services.TryAddScoped<CollectionEngineService>();

        builder.Services.TryAddScoped<ISiteCollectorManager>(provider => {
            var options = provider.GetRequiredService<IOptions<SuCaiFlowCoreOptions>>().Value;
            var service = provider.GetRequiredService<SiteCollectorManager>();
            foreach (var item in options.SiteCollectorImplementTypes) {
                if (provider.GetService(item) is ISiteCollector collector)
                    service.RegisterCollector(collector);
            }
            return service;
        });

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IPostConfigureOptions<SuCaiFlowCoreOptions>, SuCaiFlowConfiguration>());

        return new SuCaiFlowCoreBuilder(builder.Services);
    }

    public static SuCaiFlowBuilder AddCore(this SuCaiFlowBuilder builder, Action<SuCaiFlowCoreBuilder> configureOptions) {
        configureOptions(builder.AddCore());
        return builder;
    }
}
