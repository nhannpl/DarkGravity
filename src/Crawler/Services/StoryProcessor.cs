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
    private readonly IStoryAnalyzer _analyzer;

    public StoryProcessor(AppDbContext db, IStoryAnalyzer analyzer)
    {
        _db = db;
        _analyzer = analyzer;
    }

    public async Task RepairDatabaseAsync()
    {
        Console.WriteLine("🛠️ Maintenance: Syncing database scores with latest parsing logic...");

        var allStories = await _db.Stories.ToListAsync();
        int repairCount = 0;

        foreach (var story in allStories)
        {
            // Re-parse the existing text with our improved logic
            var newScore = _analyzer.ParseScore(story.AiAnalysis);

            // If the score was wrong (like the 1.0 vs 7.0 issue) or null, update it
            if (story.ScaryScore != newScore)
            {
                story.ScaryScore = newScore;
                _db.Stories.Update(story);
                repairCount++;
            }

            // Also check if the text itself contains error messages
            if (IsAnalysisInvalid(story.AiAnalysis))
            {
                Console.WriteLine($"   [RE-PROCESS] AI Analysis was invalid for: {story.Title}. Re-analyzing...");
                (story.AiAnalysis, story.ScaryScore) = await _analyzer.AnalyzeAsync(story);
                _db.Stories.Update(story);
                repairCount++;
            }
        }

        if (repairCount > 0)
        {
            await _db.SaveChangesAsync();
            Console.WriteLine($"✅ Repair complete. Updated {repairCount} records.");
        }
        else
        {
            Console.WriteLine("✨ Database is synchronized.");
        }
    }

    public async Task ProcessAndSaveStoriesAsync(List<Story> stories)
    {
        if (_db.Database.IsRelational())
        {
            await _db.Database.MigrateAsync();
        }


        foreach (var story in stories)
        {
            var existingStory = await _db.Stories.FirstOrDefaultAsync(s => s.ExternalId == story.ExternalId);

            if (existingStory != null)
            {
                if (!IsAnalysisInvalid(existingStory.AiAnalysis))
                {
                    Console.WriteLine($"   [SKIP] {story.Title}");
                    continue;
                }

                // Self-healing: Re-process if previously failed
                Console.WriteLine($"   [RE-PROCESS] Refreshing failed analysis for: {story.Title}...");
                (existingStory.AiAnalysis, existingStory.ScaryScore) = await _analyzer.AnalyzeAsync(story);
                _db.Stories.Update(existingStory);
                continue;
            }

            Console.WriteLine($"   [NEW] Processing: {story.Title}...");

            // Delegate analysis to the analyzer service
            (story.AiAnalysis, story.ScaryScore) = await _analyzer.AnalyzeAsync(story);

            _db.Stories.Add(story);
        }

        await _db.SaveChangesAsync();
    }

    private bool IsAnalysisInvalid(string? analysis)
    {
        if (string.IsNullOrWhiteSpace(analysis)) return true;

        // Treat Mock Analysis as 'invalid' for the purpose of Repair, 
        // essentially allowing us to 'upgrade' to real AI if it becomes available.
        if (analysis.StartsWith(Shared.Constants.ConfigConstants.MockAnalysisPrefix, System.StringComparison.OrdinalIgnoreCase))
            return true;

        // Check against centralized error keywords
        return Shared.Constants.ConfigConstants.ErrorKeywords.Any(k =>
            analysis.Contains(k, System.StringComparison.OrdinalIgnoreCase));
    }

}
