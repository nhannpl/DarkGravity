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
    // config
    var subreddits = new[] { "nosleep", "shortscarystories", "libraryofshadows", "scarystories" };
    var ytQueries = new[] { "MrBallen horror stories", "Lazy Masquerade horror stories", "The Dark Somnium", "Lighthouse Horror" };

    // Fetch from Reddit
    foreach (var sub in subreddits)
    {
        Console.WriteLine($"--- Fetching from Reddit (r/{sub}) ---");
        string redditUrl = $"https://www.reddit.com/r/{sub}/top.json?limit=2&t=day";
        var redditStories = await redditService.GetTopStoriesAsync(redditUrl);
        Console.WriteLine($"Found {redditStories.Count} Reddit stories in r/{sub}.");
        await processor.ProcessAndSaveStoriesAsync(redditStories);
    }

    // Fetch from YouTube
    foreach (var query in ytQueries)
    {
        Console.WriteLine($"\n--- Fetching from YouTube ({query}) ---");
        var ytStories = await youtubeService.GetStoriesFromChannelAsync(query, 2);
        Console.WriteLine($"Found {ytStories.Count} YouTube stories for '{query}'.");
        await processor.ProcessAndSaveStoriesAsync(ytStories);
    }

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
