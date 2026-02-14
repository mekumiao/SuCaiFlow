using System.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

namespace SuCaiFlow.Playwright;

public class SuCaiFlowPlaywrightBuilder(IServiceCollection services) {

    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    public SuCaiFlowPlaywrightBuilder Configure(Action<SuCaiFlowPlaywrightOptions> configuration) {
        ArgumentNullException.ThrowIfNull(configuration);
        Services.Configure(configuration);
        return this;
    }

    public SuCaiFlowPlaywrightBuilder Configure(Action<SuCaiFlowPlaywrightOptions, IServiceProvider> configuration) {
        ArgumentNullException.ThrowIfNull(configuration);
        Services.AddOptions<SuCaiFlowPlaywrightOptions>().Configure(configuration);
        return this;
    }
}
