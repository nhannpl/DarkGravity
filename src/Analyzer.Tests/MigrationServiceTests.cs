using Shared.Data;
using Shared.Models;
using Analyzer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Analyzer.Tests;

public class MigrationServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IStoryAnalyzer> _mockAnalyzer;
    private readonly Mock<ILogger<MigrationService>> _mockLogger;
    private readonly MigrationService _service;

    public MigrationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockAnalyzer = new Mock<IStoryAnalyzer>();
        _mockLogger = new Mock<ILogger<MigrationService>>();

        _service = new MigrationService(_context, _mockAnalyzer.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task MigrateMockStoriesAsync_UpdatesStory_WhenMockAnalysisExists()
    {
        // Arrange
        var story = new Story
        {
            Title = "Ghost Story",
            BodyText = "Boo!",
            AiAnalysis = "MOCK ANALYSIS: Scary! (Score: 8/10)",
            ScaryScore = 8,
            FetchedAt = DateTime.UtcNow
        };
        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        _mockAnalyzer.Setup(a => a.AnalyzeAsync(It.IsAny<Story>()))
            .ReturnsAsync(("Real Analysis: Very scary.", 9.5));

        // Act
        await _service.MigrateMockStoriesAsync();

        // Assert
        var updatedStory = await _context.Stories.FirstAsync();
        Assert.Equal("Real Analysis: Very scary.", updatedStory.AiAnalysis);
        Assert.Equal(9.5, updatedStory.ScaryScore);
    }

    [Fact]
    public async Task MigrateMockStoriesAsync_DoesNotUpdate_WhenRealAnalysisExists()
    {
        // Arrange
        var story = new Story
        {
            Title = "Real Story",
            BodyText = "Boo!",
            AiAnalysis = "Real Analysis: Scary.",
            ScaryScore = 7,
            FetchedAt = DateTime.UtcNow
        };
        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        // Act
        await _service.MigrateMockStoriesAsync();

        // Assert - Analyzer should NOT be called
        _mockAnalyzer.Verify(a => a.AnalyzeAsync(It.IsAny<Story>()), Times.Never);
    }
}
