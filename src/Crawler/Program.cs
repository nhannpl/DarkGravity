using Shared.Data;
using Shared.Models;
using Crawler.Services;

Console.WriteLine("🕷️ Spider Starting (Modular Edition)...");

// 1. Dependency Injection Setup
using var db = new AppDbContext();
using var client = new HttpClient();
client.DefaultRequestHeaders.Add("User-Agent", "MyCrawler/1.0");

var redditService = new RedditService(client);
var analyzer = new StoryAnalyzer(client);
var processor = new StoryProcessor(db, analyzer);

// 2. Execution
try
{
    Console.WriteLine("Fetching stories...");
    string url = "https://www.reddit.com/r/nosleep/top.json?limit=3&t=day";
    var stories = await redditService.GetTopStoriesAsync(url);

    Console.WriteLine($"Found {stories.Count} stories. Processing...");
    await processor.ProcessAndSaveStoriesAsync(stories);

    Console.WriteLine("\n✅ Job Complete.");
}
catch (Exception ex)
{
    Console.WriteLine($"🔥 Fatal Error: {ex.Message}");
}

// 3. View Results (Verification)
Console.WriteLine($"\n📊 VIEWING DATABASE CONTENTS:");
foreach (var s in db.Stories)
{
    Console.WriteLine($"ID: {s.Id} | {s.Title} | AI: {s.AiAnalysis}");
}
