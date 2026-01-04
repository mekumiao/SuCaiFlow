using System.ComponentModel;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SuCaiFlow.Engine;

[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed class SuCaiFlowEngineConfiguration(IServiceProvider provider) : IPostConfigureOptions<SuCaiFlowEngineOptions> {
    private readonly IServiceProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public void PostConfigure(string? name, SuCaiFlowEngineOptions options) {
        ArgumentNullException.ThrowIfNull(options);
        options.TimeProvider ??= _provider.GetService<TimeProvider>() ?? TimeProvider.System;
    }
}
