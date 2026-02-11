using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SuCaiFlow.Example;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddSuCaiFlow()
                .AddEngine(options => options
                .AddSiteCollector<ExampleSiteCollector>()
                .UseEntityFrameworkCore()
                .UseDbContext<SuCaiFlowDbContext>());

builder.Services.AddDbContext<SuCaiFlowDbContext>(options => options
                .UseInMemoryDatabase("SuCaiFlowDemo")
                .UseSuCaiFlow());

var host = builder.Build();

await host.RunAsync();
