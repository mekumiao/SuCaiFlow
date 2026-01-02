using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SuCaiFlow.Contracts.Interfaces;
using SuCaiFlow.Core.Services;

namespace SuCaiFlow.Core.Extensions;

public static class SuCaiFlowExtensions {
    public static IServiceCollection AddSuCaiFlow(this IServiceCollection services) {
        // 注册服务
        services.TryAddScoped<ICollectionTaskService, CollectionTaskService>();
        services.TryAddScoped<IAssetService, AssetService>();
        services.TryAddScoped<IEventPublisher, EventPublisher>();
        services.TryAddScoped<ITaskExecutionService, TaskExecutionService>();
        services.TryAddScoped<ISiteCollectorManager, SiteCollectorManager>();
        services.TryAddScoped<CollectionEngineService>();

        // 注册HttpClient
        //services.AddHttpClient();

        return services;
    }

    public static IServiceCollection AddSuCaiFlow(this IServiceCollection services, Action<SuCaiFlowOptions> configureOptions) {
        var options = new SuCaiFlowOptions();
        configureOptions(options);

        // 注册服务
        services.TryAddScoped<ICollectionTaskService, CollectionTaskService>();
        services.TryAddScoped<IAssetService, AssetService>();
        services.TryAddScoped<IEventPublisher, EventPublisher>();
        services.TryAddScoped<ITaskExecutionService, TaskExecutionService>();
        services.TryAddScoped<ISiteCollectorManager, SiteCollectorManager>();
        services.TryAddScoped<CollectionEngineService>();

        // 注册HttpClient
        //services.AddHttpClient();

        return services;
    }
}

public class SuCaiFlowOptions {
    public string? ConnectionString { get; set; }
    public bool EnableAutoMigrations { get; set; } = false;
    public int MaxConcurrency { get; set; } = 5;
}
