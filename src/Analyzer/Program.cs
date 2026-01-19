using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Shared.Data;
using Microsoft.EntityFrameworkCore;
using Analyzer.Services;

namespace Analyzer;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Load .env file from root
        LoadEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

        var builder = Host.CreateApplicationBuilder(args);

        // Add Configuration
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddUserSecrets<Program>();

        // Add Services
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("⚠️ WARNING: DefaultConnection not found in configuration!");
            // Look for it in a few places to be helpful
            Console.WriteLine($"Current Directory: {Environment.CurrentDirectory}");
        }
        else
        {
            Console.WriteLine("✅ Found Connection String base.");
        }

        builder.Services.AddAnalyzerServices(builder.Configuration);

        var host = builder.Build();

        Console.WriteLine("🚀 DarkGravity Analyzer Started (Event-Driven Mode)");
        Console.WriteLine("📡 Listening for StoryFetched events on Kafka...");

        await host.RunAsync();
    }

    private static void LoadEnv(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"ℹ️ No .env file found at {filePath}");
            return;
        }

        Console.WriteLine("📖 Loading environment from .env...");
        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            // Only set if not already set by environment
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
