using SuCaiFlow.Playwright;

namespace Microsoft.Extensions.DependencyInjection;

public static class SuCaiFlowPlaywrightExtensions {
    public static SuCaiFlowPlaywrightBuilder AddPlaywright(this IServiceCollection services) {
        services.AddSingleton<SuCaiFlowPlaywrightHolder>();
        services.AddHostedService<SuCaiFlowPlaywrightBackgroundService>();
        return new SuCaiFlowPlaywrightBuilder(services);
    }

    public static IServiceCollection AddPlaywright(this IServiceCollection services, Action<SuCaiFlowPlaywrightBuilder> configureOptions) {
        configureOptions(services.AddPlaywright());
        return services;
    }
}
