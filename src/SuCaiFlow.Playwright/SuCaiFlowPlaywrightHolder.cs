using Microsoft.Playwright;

namespace SuCaiFlow.Playwright;

public sealed class SuCaiFlowPlaywrightHolder {
    private readonly TaskCompletionSource _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _browserContext;
    private int _initialized; // 0 = no, 1 = yes, 2 = stopped

    internal void SetOnce(IPlaywright playwright, IBrowser browser, IBrowserContext browserContext) {
        if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
            throw new InvalidOperationException("Playwright 已初始化或已停止");

        Volatile.Write(ref _playwright, playwright);
        Volatile.Write(ref _browser, browser);
        Volatile.Write(ref _browserContext, browserContext);
        _completionSource.TrySetResult();
    }

    internal void SetException(Exception ex) {
        if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
            throw new InvalidOperationException("Playwright 已初始化或已停止");

        _completionSource.TrySetException(ex);
    }

    public async ValueTask<IPlaywright> GetRequiredPlaywrightAsync(CancellationToken ct = default) {
        if (Volatile.Read(ref _initialized) == 0)
            await _completionSource.Task.WaitAsync(ct);
        return Volatile.Read(ref _playwright) ?? throw new InvalidOperationException("Playwright 状态异常");
    }

    public async ValueTask<IBrowser> GetRequiredBrowserAsync(CancellationToken ct = default) {
        if (Volatile.Read(ref _initialized) == 0)
            await _completionSource.Task.WaitAsync(ct);
        return Volatile.Read(ref _browser) ?? throw new InvalidOperationException("Browser 状态异常");
    }

    public async ValueTask<IBrowserContext> GetRequiredBrowserContextAsync(CancellationToken ct = default) {
        if (Volatile.Read(ref _initialized) == 0)
            await _completionSource.Task.WaitAsync(ct);
        return Volatile.Read(ref _browserContext) ?? throw new InvalidOperationException("BrowserContext 状态异常");
    }

    internal void Stop() {
        Interlocked.Exchange(ref _initialized, 2);
        Volatile.Write(ref _playwright, null);
        Volatile.Write(ref _browser, null);
    }
}
