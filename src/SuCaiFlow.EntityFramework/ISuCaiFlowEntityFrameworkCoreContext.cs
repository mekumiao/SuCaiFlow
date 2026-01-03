using System.ComponentModel;

using Microsoft.EntityFrameworkCore;

namespace SuCaiFlow.EntityFrameworkCore;

[EditorBrowsable(EditorBrowsableState.Advanced)]
public interface ISuCaiFlowEntityFrameworkCoreContext {
    ValueTask<DbContext> GetDbContextAsync(CancellationToken cancellationToken);
}
