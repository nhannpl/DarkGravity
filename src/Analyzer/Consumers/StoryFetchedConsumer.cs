using MassTransit;
using DarkGravity.Contracts.Events;
using Analyzer.Services;
using Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace Analyzer.Consumers;

/// <summary>
/// This class is a "Consumer". 
/// Implementing IConsumer<StoryFetched> tells MassTransit: 
/// "Whenever a StoryFetched event arrives on Kafka, run this class."
/// </summary>
public class StoryFetchedConsumer : IConsumer<StoryFetched>
{
    private readonly AppDbContext _db;
    private readonly IStoryAnalyzer _analyzer;

    /// <summary>
    /// Dependency Injection (DI):
    /// MassTransit automatically provides the DB and Analyzer service 
    /// exactly like it used to in the old manual Program.cs setup.
    /// </summary>
    public StoryFetchedConsumer(AppDbContext db, IStoryAnalyzer analyzer)
    {
        _db = db;
        _analyzer = analyzer;
    }

    /// <summary>
    /// The "Heart" of the worker:
    /// This method replaces the manual 'foreach' loop. 
    /// MassTransit calls this for EVERY SINGLE message it gets from Kafka.
    /// </summary>
    public async Task Consume(ConsumeContext<StoryFetched> context)
    {
        // context.Message contains the ID, Title, etc. that the Crawler sent.
        var message = context.Message;

        Console.WriteLine($"🔍 Received Story: {message.Title} (ID: {message.StoryId})");

        var story = await _db.Stories.FirstOrDefaultAsync(s => s.Id == message.StoryId);

        if (story == null)
        {
            Console.WriteLine($"⚠️ Story {message.StoryId} not found in database.");
            return;
        }

        // --- SECTION: SELF-HEALING / IDEMPOTENCY ---
        // This is your "Idempotency" logic.
        // If the story already has a valid analysis (not an error), we skip it.
        // This prevents us from paying for the same AI call twice if a message is retried.
        if (!string.IsNullOrEmpty(story.AiAnalysis) &&
            !Shared.Constants.ConfigConstants.ErrorKeywords.Any(k => story.AiAnalysis.Contains(k)))
        {
            Console.WriteLine($"✅ Story {message.StoryId} is already analyzed. Skipping.");
            return;
        }

        // --- SECTION: ANALYSIS LOGIC ---
        try
        {
            Console.WriteLine($"🤖 Running AI Analysis for: {story.Title}...");

            // This is your original analyzer call!
            // We pass the story into the service you built to get the score and analysis text.
            (story.AiAnalysis, story.ScaryScore) = await _analyzer.AnalyzeAsync(story);

            _db.Stories.Update(story);
            await _db.SaveChangesAsync();

            Console.WriteLine($"✨ Analysis Complete for: {story.Title}. Score: {story.ScaryScore}/10");
        }
        catch (Exception ex)
        {
            // ERROR HANDLING & RETRY:
            // If the AI API fails, we throw an exception. 
            // MassTransit will catch this and AUTOMATICALLY retry based on our configured policy
            // (e.g., waiting 5 seconds before trying again).
            Console.WriteLine($"❌ Error analyzing story {story.Id}: {ex.Message}");
            throw;
        }
    }
}
