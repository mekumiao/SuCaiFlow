using Microsoft.Extensions.DependencyInjection;
using SuCaiFlow.Contracts.Interfaces;
using SuCaiFlow.Contracts.Services;
using SuCaiFlow.Core.Services;

namespace SuCaiFlow.Core.Extensions
{
    public static class SuCaiFlowExtensions
    {
        public static IServiceCollection AddSuCaiFlow(this IServiceCollection services)
        {
            // 注册服务
            services.AddScoped<ICollectionTaskService, CollectionTaskService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IEventPublisher, EventPublisher>();
            services.AddScoped<ITaskExecutionService, TaskExecutionService>();
            services.AddScoped<ISiteCollectorManager, SiteCollectorManager>();
            services.AddScoped<CollectionEngineService>();

            // 注册HttpClient
            services.AddHttpClient();

            return services;
        }

        public static IServiceCollection AddSuCaiFlow(this IServiceCollection services, Action<SuCaiFlowOptions> configureOptions)
        {
            var options = new SuCaiFlowOptions();
            configureOptions(options);
            
            // 注册服务
            services.AddScoped<ICollectionTaskService, CollectionTaskService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IEventPublisher, EventPublisher>();
            services.AddScoped<ITaskExecutionService, TaskExecutionService>();
            services.AddScoped<ISiteCollectorManager, SiteCollectorManager>();
            services.AddScoped<CollectionEngineService>();

            // 注册HttpClient
            services.AddHttpClient();

            return services;
        }
    }

    public class SuCaiFlowOptions
    {
        public string? ConnectionString { get; set; }
        public bool EnableAutoMigrations { get; set; } = false;
        public int MaxConcurrency { get; set; } = 5;
    }
}
