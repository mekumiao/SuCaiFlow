// 导入必要的命名空间
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuCaiFlow.Contracts.Entities;
using SuCaiFlow.Contracts.Services;
using SuCaiFlow.Core.Extensions;
using SuCaiFlow.EntityFramework.Data;
using SuCaiFlow.EntityFramework.Extensions;

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

// 构建主机
var host = builder.Build();

// 获取服务
var collectionTaskService = host.Services.GetRequiredService<ICollectionTaskService>();
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

// 获取任务信息
var task = await collectionTaskService.GetCollectionTaskByIdAsync(taskId);
if (task != null)
{
    logger.LogInformation("任务名称: {TaskName}, 状态: {Status}", task.Name, task.Status);
}

// 启动任务
var started = await collectionTaskService.StartCollectionTaskAsync(taskId);
if (started)
{
    logger.LogInformation("成功启动任务");
}

// 获取所有任务
var allTasks = await collectionTaskService.GetAllCollectionTasksAsync();
logger.LogInformation("共有 {Count} 个任务", allTasks.Count);

logger.LogInformation("SuCaiFlow演示完成！");

await host.RunAsync();
