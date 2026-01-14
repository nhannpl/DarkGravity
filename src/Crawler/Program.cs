using Crawler;

Console.WriteLine("�️ Spider Starting (Database Mode)...");

// 1. Setup Database
using var db = new AppDbContext();
Console.WriteLine("📦 Connecting to SQL Server...");
// This line automatically creates the DB if it doesn't exist (Code-First)
bool created = await db.Database.EnsureCreatedAsync();
if (created) Console.WriteLine("   -> Database created!");
else Console.WriteLine("   -> Database already exists.");

// 2. Run Crawler
using var client = new HttpClient();
client.DefaultRequestHeaders.Add("User-Agent", "MyCrawler/1.0");

var service = new RedditService(client);
string url = "https://www.reddit.com/r/nosleep/top.json?limit=5&t=day";

Console.WriteLine("Fetching stories...");
var stories = await service.GetTopStoriesAsync(url);

// 3. Save to DB
int newCount = 0;
foreach (var story in stories)
{
    // Check if we already have this story (avoid duplicates)
    var exists = db.Stories.Any(s => s.ExternalId == story.ExternalId);
    if (!exists)
    {
        db.Stories.Add(story);
        Console.WriteLine($"   [NEW] {story.Title}");
        newCount++;
    }
    else
    {
        Console.WriteLine($"   [SKIP] {story.Title}");
    }
}

// Commit changes
await db.SaveChangesAsync();
Console.WriteLine($"✅ Saved {newCount} new stories.");

// 4. READ BACK (To show you the DB)
Console.WriteLine($"\n📊 VIEWING DATABASE CONTENTS:");
var allStories = db.Stories.OrderByDescending(s => s.Upvotes).ToList();
foreach (var s in allStories)
{
    Console.WriteLine($"ID: {s.Id} | {s.Title} ({s.Upvotes} ups)");
}

Console.WriteLine("Done.");
