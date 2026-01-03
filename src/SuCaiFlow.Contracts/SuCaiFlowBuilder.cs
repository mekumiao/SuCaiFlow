using System.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

namespace SuCaiFlow.Contracts;

public sealed class SuCaiFlowBuilder(IServiceCollection services) {
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
}
