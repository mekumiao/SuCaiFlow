using Microsoft.EntityFrameworkCore;

namespace SuCaiFlow.EntityFrameworkCore;

public class SuCaiFlowEntityFrameworkCoreContext<TContext> : ISuCaiFlowEntityFrameworkCoreContext
    where TContext : DbContext {

    private readonly TContext? _context;

    public SuCaiFlowEntityFrameworkCoreContext() {
    }

    public SuCaiFlowEntityFrameworkCoreContext(TContext? context = null) {
        _context = context;
    }

    public ValueTask<DbContext> GetDbContextAsync(CancellationToken cancellationToken) {
        return cancellationToken.IsCancellationRequested
            ? new(Task.FromCanceled<DbContext>(cancellationToken))
            : _context is not DbContext context ? new(Task.FromException<DbContext>(new InvalidOperationException())) : new(context);
    }
}
