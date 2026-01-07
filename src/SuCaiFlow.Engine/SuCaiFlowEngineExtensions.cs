using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using SuCaiFlow.Abstractions;
using SuCaiFlow.Core.Services;
using SuCaiFlow.Engine;

namespace Microsoft.Extensions.DependencyInjection;

public static class SuCaiFlowEngineExtensions {
    public static SuCaiFlowEngineBuilder AddEngine(this SuCaiFlowBuilder builder) {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddLogging();
        builder.Services.AddMemoryCache();
        builder.Services.AddOptions();

        builder.Services.TryAddScoped(typeof(SuCaiFlowTaskManager<>));
        builder.Services.TryAddScoped(typeof(SuCaiFlowAssetManager<>));

        builder.Services.TryAddScoped<ISuCaiFlowTaskManager>(static provider =>
            throw new InvalidOperationException());
        builder.Services.TryAddScoped<ISuCaiFlowAssetManager>(static provider =>
            throw new InvalidOperationException());

        builder.Services.TryAddSingleton<SuCaiFlowEngineService>();
        builder.Services.TryAddSingleton<SuCaiFlowEngineTaskTracker>();
        builder.Services.TryAddSingleton<SuCaiFlowEngineConcurrencyExecutor>();
        builder.Services.TryAddSingleton<SuCaiFlowEngineSiteCollectorManager>();
        builder.Services.TryAddSingleton<ISuCaiFlowEngineEventPublisher, SuCaiFlowEngineDefaultEventPublisher>();
        builder.Services.AddHostedService<SuCaiFlowEngineExecutorHostedService>();

        builder.Services.TryAddSingleton<ISuCaiFlowEngineSiteCollectorManager>(provider => {
            var options = provider.GetRequiredService<IOptions<SuCaiFlowEngineOptions>>().Value;
            var service = provider.GetRequiredService<SuCaiFlowEngineSiteCollectorManager>();
            foreach (var item in options.SiteCollectorImplementTypes) {
                if (provider.GetService(item) is ISuCaiFlowEngineSiteCollector collector)
                    service.RegisterCollector(collector);
            }
            return service;
        });

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IPostConfigureOptions<SuCaiFlowEngineOptions>, SuCaiFlowEngineConfiguration>());

        return new SuCaiFlowEngineBuilder(builder.Services);
    }

    public static SuCaiFlowBuilder AddEngine(this SuCaiFlowBuilder builder, Action<SuCaiFlowEngineBuilder> configureOptions) {
        configureOptions(builder.AddEngine());
        return builder;
    }
}
