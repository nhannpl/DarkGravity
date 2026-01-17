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
        Console.WriteLine("🛠️ Maintenance: Scanning database for invalid analyses...");

        var badStories = await _db.Stories
            .Where(s => string.IsNullOrEmpty(s.AiAnalysis) || s.ScaryScore == null)
            .ToListAsync();

        // Also check keywords if not caught by null score
        var allStories = await _db.Stories.ToListAsync();
        var toFix = allStories.Where(s => IsAnalysisInvalid(s.AiAnalysis)).ToList();

        if (toFix.Count == 0)
        {
            Console.WriteLine("✨ Database is healthy. No corruption found.");
            return;
        }

        Console.WriteLine($"🔍 Found {toFix.Count} stories needing repair. Starting recovery...");

        foreach (var story in toFix)
        {
            Console.WriteLine($"   [REPAIR] Fixing: {story.Title}...");
            (story.AiAnalysis, story.ScaryScore) = await _analyzer.AnalyzeAsync(story);
            _db.Stories.Update(story);
        }

        await _db.SaveChangesAsync();
        Console.WriteLine("✅ Repair complete.");
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
