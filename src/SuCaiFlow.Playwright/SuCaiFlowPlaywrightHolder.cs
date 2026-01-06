using Microsoft.Playwright;

namespace SuCaiFlow.Playwright;

public sealed class SuCaiFlowPlaywrightHolder {
    private IPlaywright? _playwright;
    private int _initialized; // 0 = no, 1 = yes, 2 = stopped

    public void SetOnce(IPlaywright playwright) {
        if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0) {
            throw new InvalidOperationException("Playwright 已初始化或已停止");
        }
        Volatile.Write(ref _playwright, playwright);
    }

    public IPlaywright GetRequired() {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _initialized) != 1, this);
        return Volatile.Read(ref _playwright) ?? throw new InvalidOperationException("Playwright 状态异常");
    }

    public void Stop() {
        Interlocked.Exchange(ref _initialized, 2);
        Volatile.Write(ref _playwright, null);
    }
}
