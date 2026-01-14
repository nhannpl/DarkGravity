using Shared.Data;
using Shared.Models;
using Crawler.Services;

Console.WriteLine("🕷️ Spider Starting (Multi-Source Edition)...");

// 1. Dependency Injection Setup
using var db = new AppDbContext();
using var client = new HttpClient();
client.DefaultRequestHeaders.Add("User-Agent", "MyCrawler/1.0");

var redditService = new RedditService(client);
var youtubeService = new YouTubeService();
var analyzer = new StoryAnalyzer(client);
var processor = new StoryProcessor(db, analyzer);

// 2. Execution
try
{
    // Fetch from Reddit
    Console.WriteLine("--- Fetching from Reddit (r/nosleep) ---");
    string redditUrl = "https://www.reddit.com/r/nosleep/top.json?limit=2&t=day";
    var redditStories = await redditService.GetTopStoriesAsync(redditUrl);
    Console.WriteLine($"Found {redditStories.Count} Reddit stories.");
    await processor.ProcessAndSaveStoriesAsync(redditStories);

    // Fetch from YouTube (MrBallen)
    Console.WriteLine("\n--- Fetching from YouTube (MrBallen) ---");
    var ytStories = await youtubeService.GetStoriesFromChannelAsync("MrBallen horror stories", 2);
    Console.WriteLine($"Found {ytStories.Count} YouTube stories.");
    await processor.ProcessAndSaveStoriesAsync(ytStories);

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
    Console.WriteLine($"[{s.Author}] {s.Title} | Score: {s.ScaryScore}");
}
