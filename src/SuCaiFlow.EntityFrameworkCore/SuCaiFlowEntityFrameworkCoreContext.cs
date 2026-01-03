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
        if (cancellationToken.IsCancellationRequested) {
            return new(Task.FromCanceled<DbContext>(cancellationToken));
        }

        if (_context is not DbContext context) {
            return new(Task.FromException<DbContext>(new InvalidOperationException()));
        }

        return new(context);
    }
}
