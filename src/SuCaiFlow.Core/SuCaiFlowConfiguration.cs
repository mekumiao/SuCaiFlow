using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SuCaiFlow.Core;

public class SuCaiFlowConfiguration(IServiceProvider provider) : IPostConfigureOptions<SuCaiFlowCoreOptions> {
    private readonly IServiceProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public void PostConfigure(string? name, SuCaiFlowCoreOptions options) {
        options.TimeProvider ??= _provider.GetService<TimeProvider>() ?? TimeProvider.System;
    }
}
