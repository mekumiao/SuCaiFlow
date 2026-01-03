#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
using SuCaiFlow.Contracts;

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
