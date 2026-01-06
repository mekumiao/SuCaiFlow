using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SuCaiFlow.Engine;
using SuCaiFlow.EntityFrameworkCore;
using SuCaiFlow.EntityFrameworkCore.Models;

namespace Microsoft.Extensions.DependencyInjection;

public static class SuCaiFlowEntityFrameworkCoreExtensions {

    public static SuCaiFlowEntityFrameworkCoreBuilder UseEntityFrameworkCore(this SuCaiFlowEngineBuilder builder) {
        ArgumentNullException.ThrowIfNull(builder);

        builder.SetDefaultTaskEntity<SuCaiFlowEntityFrameworkCoreTask>()
               .SetDefaultAssetEntity<SuCaiFlowEntityFrameworkCoreAsset>();

        builder.ReplaceTaskStore<SuCaiFlowEntityFrameworkCoreTask, SuCaiFlowEntityFrameworkCoreTaskStore>()
               .ReplaceAssetStore<SuCaiFlowEntityFrameworkCoreAsset, SuCaiFlowEntityFrameworkCoreAssetStore>();

        builder.Services.TryAddScoped<ISuCaiFlowEntityFrameworkCoreContext>(static provider =>
            throw new InvalidOperationException("未注册DbContext"));

        return new SuCaiFlowEntityFrameworkCoreBuilder(builder.Services);
    }

    public static SuCaiFlowEngineBuilder UseEntityFrameworkCore(this SuCaiFlowEngineBuilder builder, Action<SuCaiFlowEntityFrameworkCoreBuilder> configureOptions) {
        configureOptions(builder.UseEntityFrameworkCore());
        return builder;
    }
}
