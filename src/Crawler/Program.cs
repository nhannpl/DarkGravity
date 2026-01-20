using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using Crawler.Services;
using Crawler;
using MassTransit;
using Shared.Helpers;

// Load .env file from root
EnvLoader.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

Console.WriteLine("🕷️ Spider Starting (Multi-Source Edition)...");

// 1. Host Setup
var builder = Host.CreateApplicationBuilder(args);

// Add Configuration
builder.Configuration.SetBasePath(AppContext.BaseDirectory);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddCrawlerServices(builder.Configuration);

var host = builder.Build();

// 2. Execution
await host.StartAsync();

Console.WriteLine("🚌 Bus starting...");
await Task.Delay(2000); // Give it a moment to stabilize

try
{
    Console.WriteLine("🚀 Running Crawler App...");
    using var scope = host.Services.CreateScope();
    var app = scope.ServiceProvider.GetRequiredService<ICrawlerApp>();
    await app.RunAsync();
}
finally
{
    await host.StopAsync();
}
