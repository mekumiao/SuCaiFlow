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
    private IBrowser? _browser;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", $"{AppContext.BaseDirectory}/pw-browsers");
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {
                Headless = true,
            });
            _playwrightHolder.SetOnce(_playwright, _browser);
            _logger.LogDebug("已启动 Playwright");
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
            _logger.LogDebug("Playwright Host Service 收到停止信号");
        }
        catch (Exception ex) {
            _playwrightHolder.SetException(ex);
            _logger.LogError(ex, "Playwright Host Service 启动失败");
        }
        finally {
            if (_browser != null)
                await _browser.DisposeAsync();
            _playwrightHolder.Stop();
            _playwright?.Dispose();
            _playwright = null;
            _browser = null;
            _logger.LogDebug("Playwright 已停止");
        }
    }
}
