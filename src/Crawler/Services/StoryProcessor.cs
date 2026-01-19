using Shared.Data;
using Shared.Models;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using DarkGravity.Contracts.Events;

namespace Crawler.Services;

public interface IStoryProcessor
{
    Task ProcessAndSaveStoriesAsync(List<Story> stories);
    Task RepairDatabaseAsync();
}


public class StoryProcessor : IStoryProcessor
{
    private readonly AppDbContext _db;
    private readonly ITopicProducer<StoryFetched> _producer;

    public StoryProcessor(AppDbContext db, ITopicProducer<StoryFetched> producer)
    {
        _db = db;
        _producer = producer;
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
        var newStories = new List<Story>();

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
            newStories.Add(story);
            newCount++;
        }

        if (newCount > 0)
        {
            await _db.SaveChangesAsync();

            // Fire-and-forget events for the Analyzer
            foreach (var story in newStories)
            {
                await _producer.Produce(new StoryFetched(
                    story.Id,
                    story.Title,
                    story.BodyText,
                    story.Url
                ));
            }

            Console.WriteLine($"✅ Saved {newCount} new stories and triggered analysis events.");
        }

        if (skippedCount > 0)
        {
            Console.WriteLine($"ℹ️ Skipped {skippedCount} existing stories.");
        }
    }
}
