#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SuCaiFlow.Core;
using SuCaiFlow.EntityFrameworkCore;
using SuCaiFlow.EntityFrameworkCore.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class SuCaiFlowEntityFrameworkCoreExtensions {

    public static SuCaiFlowEntityFrameworkCoreBuilder UseEntityFrameworkCore(this SuCaiFlowCoreBuilder builder) {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ReplaceCollectedAssetRepository<CollectedAssetRepository>()
               .ReplaceCollectionTaskRepository<CollectionTaskRepository>()
               .ReplaceCollectionTaskConfigRepository<CollectionTaskConfigRepository>();

        builder.Services.TryAddScoped<ISuCaiFlowEntityFrameworkCoreContext>(static provider =>
            throw new InvalidOperationException("未注册DbContext"));

        return new SuCaiFlowEntityFrameworkCoreBuilder(builder.Services);
    }
}
