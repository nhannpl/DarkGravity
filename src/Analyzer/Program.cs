using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Shared.Data;
using Microsoft.EntityFrameworkCore;
using Analyzer.Services;
using Shared.Helpers;

namespace Analyzer;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Load .env file from root
        EnvLoader.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

        var builder = Host.CreateApplicationBuilder(args);

        // Add Configuration
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddUserSecrets<Program>();

        // Add Services
        builder.Services.AddAnalyzerServices(builder.Configuration);

        var host = builder.Build();



        if (args.Contains("--migrate"))
        {
            Console.WriteLine("🚀 Running Migration: Mock Analysis -> Real Analysis");
            using var scope = host.Services.CreateScope();
            var migrationService = scope.ServiceProvider.GetRequiredService<MigrationService>();
            await migrationService.MigrateMockStoriesAsync();
            Console.WriteLine("👋 Migration Finished. Exiting.");
            return;
        }

        Console.WriteLine("🚀 DarkGravity Analyzer Started (Event-Driven Mode)");
        Console.WriteLine("📡 Listening for StoryFetched events on Kafka...");

        await host.RunAsync();
    }
}
