// 导入必要的命名空间
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Interfaces;
using SuCaiFlow.Core.Extensions;
using SuCaiFlow.EntityFramework.Data;
using SuCaiFlow.EntityFramework.Extensions;
using SuCaiFlow.Example;

// 创建主机构建器
var builder = Host.CreateApplicationBuilder(args);

// 配置日志
builder.Logging.AddConsole();

// 添加SuCaiFlow服务
builder.Services.AddSuCaiFlow();

// 添加SuCaiFlow数据库上下文和仓储
builder.Services.AddSuCaiFlowDbContext<SuCaiFlowDbContext>(options =>
    options.UseInMemoryDatabase("SuCaiFlowDemo"));

builder.Services.AddSuCaiFlowRepositories();

// 注册示例站点采集器
builder.Services.AddScoped<ISiteCollector, ExampleSiteCollector>();

// 构建主机
var host = builder.Build();

// 获取服务
var collectionTaskService = host.Services.GetRequiredService<ICollectionTaskService>();
var taskExecutionService = host.Services.GetRequiredService<ITaskExecutionService>();
var eventPublisher = host.Services.GetRequiredService<IEventPublisher>();
var siteCollectorManager = host.Services.GetRequiredService<ISiteCollectorManager>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("开始演示SuCaiFlow功能...");

// 创建一个采集任务
var taskId = await collectionTaskService.CreateCollectionTaskAsync(
    "示例任务",
    "这是一个示例采集任务",
    "https://example.com",
    ".item",
    new Dictionary<string, string> { { "key", "value" } });

logger.LogInformation("创建采集任务，ID: {TaskId}", taskId);

// 检查任务执行状态
var isRunning = await taskExecutionService.IsTaskRunningAsync(taskId);
logger.LogInformation("任务运行状态: {IsRunning}", isRunning);

// 获取任务信息
var task = await collectionTaskService.GetCollectionTaskByIdAsync(taskId);
if (task != null) {
    logger.LogInformation("任务名称: {TaskName}, 状态: {Status}", task.Name, task.Status);
}

// 启动任务
var started = await collectionTaskService.StartCollectionTaskAsync(taskId);
if (started) {
    logger.LogInformation("成功启动任务");

    // 再次检查任务执行状态
    isRunning = await taskExecutionService.IsTaskRunningAsync(taskId);
    logger.LogInformation("任务运行状态: {IsRunning}", isRunning);
}

// 获取所有任务
var allTasks = await collectionTaskService.GetAllCollectionTasksAsync();
logger.LogInformation("共有 {Count} 个任务", allTasks.Count);

// 演示事件发布功能
await eventPublisher.PublishAsync(new SuCaiFlow.Contracts.Events.CollectionTaskStartedEvent {
    TaskId = taskId,
    TaskName = "示例任务",
    Timestamp = DateTime.UtcNow
});

logger.LogInformation("演示站点采集器管理器功能...");
logger.LogInformation("当前注册的采集器数量: {Count}", siteCollectorManager.GetAllCollectors().Count);

// 注册示例采集器
var exampleCollector = new ExampleSiteCollector();
siteCollectorManager.RegisterCollector(exampleCollector);

logger.LogInformation("注册示例采集器后，当前采集器数量: {Count}", siteCollectorManager.GetAllCollectors().Count);

// 尝试获取采集器
var collector = siteCollectorManager.GetCollectorForUrl("https://example.com/page");
if (collector != null) {
    logger.LogInformation("找到采集器: {DisplayName} ({SiteIdentifier})", collector.DisplayName, collector.SiteIdentifier);

    // 尝试解析页面
    var urls = await collector.ParsePageAsync(task!, "https://example.com/page", null);
    logger.LogInformation("从页面解析到 {Count} 个URL", urls.Count);

    foreach (var url in urls.Take(2)) // 只下载前两个资源以节省时间
    {
        var asset = await collector.DownloadAssetAsync(url);
        if (asset != null) {
            logger.LogInformation("下载资源成功: {Url} -> {LocalPath}", asset.Url, asset.LocalPath);
        }
    }
}

logger.LogInformation("SuCaiFlow演示完成！");

await host.RunAsync();
