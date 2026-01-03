using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SuCaiFlow.EntityFrameworkCore;

public sealed class SuCaiFlowEntityFrameworkCoreCustomizer(ModelCustomizerDependencies dependencies)
    : RelationalModelCustomizer(dependencies) {
    public override void Customize(ModelBuilder modelBuilder, DbContext context) {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(context);

        modelBuilder.UseSuCaiFlow();

        base.Customize(modelBuilder, context);
    }
}
