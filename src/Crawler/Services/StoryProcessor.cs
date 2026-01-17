using Shared.Data;
using Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Crawler.Services;

public interface IStoryProcessor
{
    Task ProcessAndSaveStoriesAsync(List<Story> stories);
    Task RepairDatabaseAsync();
}


public class StoryProcessor : IStoryProcessor
{
    private readonly AppDbContext _db;

    public StoryProcessor(AppDbContext db)
    {
        _db = db;
    }

    public async Task RepairDatabaseAsync()
    {
        // This functionality is being moved to the Analyzer project.
        // For now, we keep the interface method but leave it empty or print a redirect message.
        Console.WriteLine("🛠️ Note: Database repair and re-analysis has moved to the Analyzer project.");
        await Task.CompletedTask;
    }

    public async Task ProcessAndSaveStoriesAsync(List<Story> stories)
    {
        if (_db.Database.IsRelational())
        {
            await _db.Database.MigrateAsync();
        }

        int newCount = 0;
        int skippedCount = 0;

        foreach (var story in stories)
        {
            var existingStory = await _db.Stories.FirstOrDefaultAsync(s => s.ExternalId == story.ExternalId);

            if (existingStory != null)
            {
                skippedCount++;
                continue;
            }

            Console.WriteLine($"   [NEW] Found: {story.Title}...");

            // Set analysis to empty - the Analyzer project will pick this up
            story.AiAnalysis = string.Empty;
            story.ScaryScore = null;

            _db.Stories.Add(story);
            newCount++;
        }

        if (newCount > 0)
        {
            await _db.SaveChangesAsync();
            Console.WriteLine($"✅ Saved {newCount} new stories. Run the Analyzer project to process them.");
        }

        if (skippedCount > 0)
        {
            Console.WriteLine($"ℹ️ Skipped {skippedCount} existing stories.");
        }
    }
}
