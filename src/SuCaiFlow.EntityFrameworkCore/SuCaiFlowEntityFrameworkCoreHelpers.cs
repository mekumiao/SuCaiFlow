#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
using Microsoft.EntityFrameworkCore.Infrastructure;

using SuCaiFlow.EntityFrameworkCore;
using SuCaiFlow.EntityFrameworkCore.Configurations;

namespace Microsoft.EntityFrameworkCore;

public static class SuCaiFlowEntityFrameworkCoreHelpers {
    public static DbContextOptionsBuilder<TContext> UseOpenIddict<TContext>(this DbContextOptionsBuilder<TContext> builder)
       where TContext : DbContext {
        builder.UseSuCaiFlow();
        return builder;
    }

    public static DbContextOptionsBuilder UseSuCaiFlow(this DbContextOptionsBuilder builder) {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ReplaceService<IModelCustomizer, SuCaiFlowEntityFrameworkCoreCustomizer>();
    }

    public static ModelBuilder UseSuCaiFlow(this ModelBuilder builder) {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .ApplyConfiguration(new CollectionTaskConfiguration())
            .ApplyConfiguration(new CollectedAssetConfiguration())
            .ApplyConfiguration(new CollectionTaskConfigConfiguration());
    }
}
