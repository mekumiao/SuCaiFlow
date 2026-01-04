using SuCaiFlow.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class SuCaiFlowExtensions {
    public static SuCaiFlowBuilder AddSuCaiFlow(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        return new SuCaiFlowBuilder(services);
    }

    public static IServiceCollection AddSuCaiFlow(this IServiceCollection services, Action<SuCaiFlowBuilder> configuration) {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        configuration(services.AddSuCaiFlow());

        return services;
    }
}
