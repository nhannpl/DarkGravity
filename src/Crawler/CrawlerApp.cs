using Crawler.Services;
using Shared.Data;

namespace Crawler;

public interface ICrawlerApp
{
    Task RunAsync();
}

public class CrawlerApp : ICrawlerApp
{
    private readonly IRedditService _reddit;
    private readonly IYouTubeService _youtube;
    private readonly IStoryProcessor _processor;
    private readonly AppDbContext _db;

    public CrawlerApp(
        IRedditService reddit,
        IYouTubeService youtube,
        IStoryProcessor processor,
        AppDbContext db)
    {
        _reddit = reddit;
        _youtube = youtube;
        _processor = processor;
        _db = db;
    }

    public async Task RunAsync()
    {
        try
        {
            // Maintenance: Fix any corrupted data from previous failed AI runs
            await _processor.RepairDatabaseAsync();

            var subreddits = new[] { "nosleep", "shortscarystories", "libraryofshadows", "scarystories" };

            var ytQueries = new[] { "MrBallen horror stories", "Lazy Masquerade horror stories", "The Dark Somnium", "Lighthouse Horror" };

            // Fetch from Reddit
            foreach (var sub in subreddits)
            {
                Console.WriteLine($"--- Fetching from Reddit (r/{sub}) ---");
                string redditUrl = $"https://www.reddit.com/r/{sub}/top.json?limit=2&t=day";
                var redditStories = await _reddit.GetTopStoriesAsync(redditUrl);
                Console.WriteLine($"Found {redditStories.Count} Reddit stories in r/{sub}.");
                await _processor.ProcessAndSaveStoriesAsync(redditStories);
            }

            // Fetch from YouTube
            foreach (var query in ytQueries)
            {
                Console.WriteLine($"\n--- Fetching from YouTube ({query}) ---");
                var ytStories = await _youtube.GetStoriesFromChannelAsync(query, 2);
                Console.WriteLine($"Found {ytStories.Count} YouTube stories for '{query}'.");
                await _processor.ProcessAndSaveStoriesAsync(ytStories);
            }

            Console.WriteLine("\n✅ Job Complete.");

            Console.WriteLine($"\n📊 VIEWING DATABASE CONTENTS:");
            foreach (var s in _db.Stories)
            {
                Console.WriteLine($"[{s.Author}] {s.Title} | Score: {s.ScaryScore}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 Fatal Error: {ex.Message}");
            throw; // Re-throw to let the caller know
        }
    }
}
