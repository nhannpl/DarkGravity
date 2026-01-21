using Shared.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;
using Microsoft.Extensions.Logging;

namespace Analyzer.Services;

public class MigrationService
{
    private readonly AppDbContext _context;
    private readonly IStoryAnalyzer _analyzer;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(AppDbContext context, IStoryAnalyzer analyzer, ILogger<MigrationService> logger)
    {
        _context = context;
        _analyzer = analyzer;
        _logger = logger;
    }

    public async Task MigrateMockStoriesAsync()
    {
        _logger.LogInformation("🔍 Starting migration of Mock Analysis stories...");

        // 1. Find stories with Mock Analysis
        // We look for stories containing the prefix or just "MOCK ANALYSIS"
        var mockStories = await _context.Stories
            .Where(s => s.AiAnalysis.Contains("MOCK ANALYSIS"))
            .ToListAsync();

        if (!mockStories.Any())
        {
            _logger.LogInformation("✅ No stories found requiring migration.");
            return;
        }

        _logger.LogInformation($"📝 Found {mockStories.Count} stories to migrate.");

        int successCount = 0;
        int failureCount = 0;

        foreach (var story in mockStories)
        {
            _logger.LogInformation($"🔄 Reprocessing Story ID {story.Id}: '{story.Title}'...");

            try
            {
                // 2. Re-Analyze
                var result = await _analyzer.AnalyzeAsync(story);

                // 3. Update Story
                // Only update if we didn't get MOCK ANALYSIS back again (meaning all keys failed again)
                if (result.Analysis.Contains(ConfigConstants.MockAnalysisPrefix))
                {
                    _logger.LogWarning($"⚠️ Still got Mock Analysis for '{story.Title}'. Check your API keys/Quota.");
                    failureCount++;
                }
                else
                {
                    story.AiAnalysis = result.Analysis;
                    story.ScaryScore = result.Score;


                    _logger.LogInformation($"✅ Successfully re-analyzed '{story.Title}'. Score: {story.ScaryScore}");
                    successCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error processing '{story.Title}'");
                failureCount++;
            }

            // Small delay to be nice to rate limits
            await Task.Delay(1000);
        }

        // 4. Save Changes
        await _context.SaveChangesAsync();

        _logger.LogInformation("------------------------------------------------");
        _logger.LogInformation($"🎉 Migration Complete.");
        _logger.LogInformation($"✅ Success: {successCount}");
        _logger.LogInformation($"❌ Failed/Skipped: {failureCount}");
        _logger.LogInformation("------------------------------------------------");
    }
}
