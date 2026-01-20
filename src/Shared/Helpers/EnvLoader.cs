using System;
using System.IO;

namespace Shared.Helpers;

public static class EnvLoader
{
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"ℹ️ No .env file found at {filePath}");
            return;
        }

        Console.WriteLine("📖 Loading environment from .env...");
        foreach (var line in File.ReadAllLines(filePath))
        {
            // Skip comments
            if (line.TrimStart().StartsWith("#")) continue;

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
