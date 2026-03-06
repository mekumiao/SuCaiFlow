# SuCaiFlow

SuCaiFlow 是一个素材采集流程库，旨在提供一个灵活、可扩展的素材采集框架，可以轻松集成到任何.NET 应用程序中。

## 功能特性

- **模块化设计**: 分为 Contracts、Core 和 EntityFrameworkCore 三个组件，便于独立使用
- **EF Core 集成**: 提供完整的 EF Core 支持，包括 PostgreSQL 等数据库
- **事件驱动**: 内置事件发布机制，便于扩展和监控
- **多站点支持**: 支持多种站点的采集器，可轻松扩展
- **任务管理**: 完整的采集任务生命周期管理
- **并发控制**: 支持并发采集任务执行

## 安装

```xml
<PackageReference Include="SuCaiFlow.EntityFrameworkCore" Version="1.0.0" />
```

或者通过项目引用方式使用。

## 快速开始

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SuCaiFlow.Contracts.Services;
using SuCaiFlow.Core;
using SuCaiFlow.Example;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSuCaiFlow()
                .AddCore(options => options
                .UseEntityFrameworkCore()
                .UseDbContext<SuCaiFlowDbContext>());

builder.Services.AddDbContext<SuCaiFlowDbContext>(options => options
                .UseInMemoryDatabase("SuCaiFlowDemo")
                .UseSuCaiFlow());

var host = builder.Build();

// 使用服务
var collectionTaskService = host.Services.GetRequiredService<ICollectionTaskService>();

// 创建采集任务
var taskId = await collectionTaskService.CreateCollectionTaskAsync(
"示例任务",
"这是一个示例采集任务",
"<https://example.com>",
".item",
new Dictionary<string, string> { { "key", "value" } });

// 启动任务
await collectionTaskService.StartCollectionTaskAsync(taskId);
```

## 核心组件

### Abstractions

包含所有实体和接口定义:

- 实体: CollectionTask, CollectedAsset, CollectionTaskConfig
- 接口: ICollectionTaskService, IAssetService, ISiteCollector 等

### Engine

包含业务逻辑实现:

- 服务: CollectionTaskService, AssetService, EventPublisher
- 基础类: BaseSiteCollector
- 扩展方法: SuCaiFlowExtensions

### EntityFrameworkCore

包含数据访问实现:

- DbContext: SuCaiFlowDbContext
- 仓储: CollectionTaskRepository, AssetRepository
- EF Core 扩展方法

## 注意事项

1. 由于采集功能用到了`SuCaiFlow.Playwright`，所以部署到linux服务器时，需要给node执行权限`.playwright/node/linux-x64/node`

## 扩展功能

### 添加自定义站点采集器

```csharp
public class CustomSiteCollector : BaseSiteCollector
{
public override string SiteIdentifier => "customsite.com";
public override string DisplayName => "自定义站点采集器";

    public override async Task<List<string>> ParsePageAsync(CollectionTask task, string pageUrl, CollectionTaskConfig? config = null)
    {
        // 实现页面解析逻辑
        return new List<string>();
    }

    public override async Task<CollectedAsset?> DownloadAssetAsync(string url, CollectionTaskConfig? config = null)
    {
        // 实现资源下载逻辑
        return new CollectedAsset();
    }

}
```

然后在服务注册中添加:

```csharp
builder.Services.AddScoped<ISiteCollector, CustomSiteCollector>();
```

## 依赖注入扩展

SuCaiFlow 提供以下依赖注入扩展方法:

- AddSuCaiFlow(): 注册所有核心服务
- AddSuCaiFlowDbContext\<T>(): 注册数据库上下文
- AddSuCaiFlowRepositories(): 注册仓储实现

## 发布 Nuget

```ps1
dotnet pack -c Release -o C:/App/Nuget/local
```

## 许可证

MIT
