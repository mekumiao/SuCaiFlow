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

        builder.Services.TryAddScoped<SuCaiFlowEngineService>();
        builder.Services.TryAddScoped<SuCaiFlowTaskStatusReporter>();

        builder.Services.TryAddSingleton<SuCaiFlowTaskRunner>();
        builder.Services.TryAddSingleton<SuCaiFlowTaskRegistry>();
        builder.Services.TryAddSingleton<SuCaiFlowTaskScheduler>();
        builder.Services.TryAddSingleton<ISuCaiFlowEngineEventPublisher, SuCaiFlowEngineDefaultEventPublisher>();

        builder.Services.AddHostedService<SuCaiFlowEngineHostedService>();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IPostConfigureOptions<SuCaiFlowEngineOptions>, SuCaiFlowEngineConfiguration>());

        return new SuCaiFlowEngineBuilder(builder.Services);
    }

    public static SuCaiFlowBuilder AddEngine(this SuCaiFlowBuilder builder, Action<SuCaiFlowEngineBuilder> configureOptions) {
        configureOptions(builder.AddEngine());
        return builder;
    }
}
