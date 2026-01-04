using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SuCaiFlow.Abstractions;

namespace SuCaiFlow.Engine;

public class SuCaiFlowEngineBuilder(IServiceCollection services) {

    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    public SuCaiFlowEngineBuilder Configure(Action<SuCaiFlowEngineOptions> configuration) {
        ArgumentNullException.ThrowIfNull(configuration);
        Services.Configure(configuration);
        return this;
    }

    public SuCaiFlowEngineBuilder SetMaxConcurrency(int maxConcurrency)
        => Configure(options => options.MaxConcurrency = maxConcurrency);

    public SuCaiFlowEngineBuilder AddSiteCollector<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSiteCollector>()
        where TSiteCollector : class, ISuCaiFlowEngineSiteCollector {
        Configure(options => options.SiteCollectorImplementTypes.Add(typeof(TSiteCollector)));
        Services.AddScoped<ISuCaiFlowEngineSiteCollector, TSiteCollector>();
        return this;
    }

    public SuCaiFlowEngineBuilder SetDefaultEventPublisher<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEventPublisher>(
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TEventPublisher : ISuCaiFlowEngineEventPublisher {
        Services.Replace(ServiceDescriptor.Describe(typeof(ISuCaiFlowEngineEventPublisher), typeof(TEventPublisher), lifetime));
        return this;
    }

    public SuCaiFlowEngineBuilder ReplaceAssetManager<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAsset,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TManager>()
        where TAsset : class
        where TManager : SuCaiFlowAssetManager<TAsset> {
        Services.Replace(ServiceDescriptor.Scoped<SuCaiFlowAssetManager<TAsset>>(static provider =>
            provider.GetRequiredService<TManager>()));
        return this;
    }

    public SuCaiFlowEngineBuilder ReplaceTaskManager<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TTask,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TManager>()
        where TTask : class
        where TManager : SuCaiFlowTaskManager<TManager> {
        Services.Replace(ServiceDescriptor.Scoped<SuCaiFlowTaskManager<TManager>>(static provider =>
            provider.GetRequiredService<TManager>()));
        return this;
    }

    public SuCaiFlowEngineBuilder ReplaceAssetStore<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAsset,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TStore>(
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TAsset : class
        where TStore : ISuCaiFlowAssetStore<TAsset> {
        Services.Replace(ServiceDescriptor.Describe(typeof(ISuCaiFlowAssetStore<TAsset>), typeof(TStore), lifetime));
        return this;
    }

    public SuCaiFlowEngineBuilder ReplaceTaskStore<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TTask,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TStore>(
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TTask : class
        where TStore : ISuCaiFlowTaskStore<TTask> {
        Services.Replace(ServiceDescriptor.Describe(typeof(ISuCaiFlowTaskStore<TTask>), typeof(TStore), lifetime));
        return this;
    }

    public SuCaiFlowEngineBuilder SetDefaultAssetEntity<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAsset>() where TAsset : class {
        Services.Replace(ServiceDescriptor.Scoped<ISuCaiFlowAssetManager>(static provider =>
            provider.GetRequiredService<SuCaiFlowAssetManager<TAsset>>()));
        return this;
    }

    public SuCaiFlowEngineBuilder SetDefaultTaskEntity<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TTask>() where TTask : class {
        Services.Replace(ServiceDescriptor.Scoped<ISuCaiFlowTaskManager>(static provider =>
            provider.GetRequiredService<SuCaiFlowTaskManager<TTask>>()));
        return this;
    }
}
