using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SuCaiFlow.Abstractions;
using SuCaiFlow.Engine;
using SuCaiFlow.EntityFrameworkCore.Models;

namespace SuCaiFlow.EntityFrameworkCore;

public class SuCaiFlowEntityFrameworkCoreBuilder(IServiceCollection services) {

    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    public SuCaiFlowEntityFrameworkCoreBuilder Configure(Action<SuCaiFlowEntityFrameworkCoreOptions> configuration) {
        ArgumentNullException.ThrowIfNull(configuration);

        Services.Configure(configuration);

        return this;
    }

    public SuCaiFlowEntityFrameworkCoreBuilder ReplaceDefaultEntities<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey>()
         where TKey : notnull, IEquatable<TKey> {
        return ReplaceDefaultEntities<SuCaiFlowEntityFrameworkCoreTask<TKey>, SuCaiFlowEntityFrameworkCoreAsset<TKey>, TKey>();
    }

    public SuCaiFlowEntityFrameworkCoreBuilder ReplaceDefaultEntities<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TTask,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAsset,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey>()
        where TTask : SuCaiFlowEntityFrameworkCoreTask<TKey, TAsset>
        where TAsset : SuCaiFlowEntityFrameworkCoreAsset<TKey, TTask>
        where TKey : notnull, IEquatable<TKey> {
#if SUPPORTS_TYPE_DESCRIPTOR_TYPE_REGISTRATION
        if (typeof(TKey) != typeof(string)) {
            TypeDescriptor.RegisterType<TKey>();
        }
#endif
        Services.Replace(ServiceDescriptor.Scoped<ISuCaiFlowTaskManager>(static provider =>
            provider.GetRequiredService<SuCaiFlowTaskManager<TTask>>()));
        Services.Replace(ServiceDescriptor.Scoped<ISuCaiFlowAssetManager>(static provider =>
            provider.GetRequiredService<SuCaiFlowAssetManager<TAsset>>()));

        Services.Replace(ServiceDescriptor.Scoped<ISuCaiFlowTaskStore<TTask>,
            SuCaiFlowEntityFrameworkCoreTaskStore<TTask, TAsset, TKey>>());
        Services.Replace(ServiceDescriptor.Scoped<ISuCaiFlowAssetStore<TAsset>,
            SuCaiFlowEntityFrameworkCoreAssetStore<TAsset, TTask, TKey>>());
        return this;
    }

    public SuCaiFlowEntityFrameworkCoreBuilder UseDbContext<TContext>()
        where TContext : DbContext {

        Services.Replace(ServiceDescriptor.Scoped<
            ISuCaiFlowEntityFrameworkCoreContext, SuCaiFlowEntityFrameworkCoreContext<TContext>>());

        return this;
    }
}
