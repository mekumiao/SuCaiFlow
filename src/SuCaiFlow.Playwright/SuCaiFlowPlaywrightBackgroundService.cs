using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace SuCaiFlow.Playwright;

public class SuCaiFlowPlaywrightBackgroundService(
    ILogger<SuCaiFlowPlaywrightBackgroundService> logger,
    SuCaiFlowPlaywrightHolder playwrightHolder) : BackgroundService {
    private readonly ILogger<SuCaiFlowPlaywrightBackgroundService> _logger = logger;
    private readonly SuCaiFlowPlaywrightHolder _playwrightHolder = playwrightHolder;
    private IPlaywright? _playwright;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _playwrightHolder.SetOnce(_playwright);
            _logger.LogDebug("已启动 Playwright");
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) {
            _logger.LogDebug("Playwright Host Service 收到停止信号");
        }
        finally {
            _playwrightHolder.Stop();
            _playwright?.Dispose();
            _playwright = null;
            _logger.LogDebug("Playwright 已停止");
        }
    }
}
