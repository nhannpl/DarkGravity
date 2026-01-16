using Shared.Data;
using Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Crawler.Services;

public interface IStoryProcessor
{
    Task ProcessAndSaveStoriesAsync(List<Story> stories);
}

public class StoryProcessor : IStoryProcessor
{
    private readonly AppDbContext _db;
    private readonly IStoryAnalyzer _analyzer;

    public StoryProcessor(AppDbContext db, IStoryAnalyzer analyzer)
    {
        _db = db;
        _analyzer = analyzer;
    }

    public async Task ProcessAndSaveStoriesAsync(List<Story> stories)
    {
        if (_db.Database.IsRelational())
        {
            await _db.Database.MigrateAsync();
        }

        foreach (var story in stories)
        {
            var exists = _db.Stories.Any(s => s.ExternalId == story.ExternalId);
            if (exists)
            {
                Console.WriteLine($"   [SKIP] {story.Title}");
                continue;
            }

            Console.WriteLine($"   [NEW] Processing: {story.Title}...");
            
            // Delegate analysis to the analyzer service
            // Delegate analysis to the analyzer service
            (story.AiAnalysis, story.ScaryScore) = await _analyzer.AnalyzeAsync(story);

            _db.Stories.Add(story);
        }

        await _db.SaveChangesAsync();
    }
}
