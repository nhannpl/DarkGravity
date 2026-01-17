using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Crawler.Services;
using Shared.Data;
using Shared.Models;
using Crawler;

Console.WriteLine("🕷️ Spider Starting (Multi-Source Edition)...");

// 0. Configuration Setup
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

// 1. Dependency Injection Setup
var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(config);
services.AddCrawlerServices(config);

var serviceProvider = services.BuildServiceProvider();

// Resolve root services
var app = serviceProvider.GetRequiredService<ICrawlerApp>();

// 2. Execution
await app.RunAsync();
