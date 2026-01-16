using Crawler.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Data;
using Shared.Models;
using Xunit;

namespace Crawler.Tests;

public class StoryProcessorTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public StoryProcessorTests()
    {
        // Use In-Memory database for testing
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task ProcessAndSaveStoriesAsync_SavesNewStoriesAndSkipsDuplicates()
    {
        // Arrange
        using var db = new AppDbContext(_dbOptions);

        // Add an existing story
        var existingExternalId = "ext_123";
        db.Stories.Add(new Story
        {
            ExternalId = existingExternalId,
            Title = "Old Story",
            BodyText = "Old Content",
            Author = "Author"
        });
        await db.SaveChangesAsync();

        var mockAnalyzer = new Mock<IStoryAnalyzer>();
        mockAnalyzer.Setup(a => a.AnalyzeAsync(It.IsAny<Story>()))
            .ReturnsAsync(("AI Analysis", 75.0));

        var processor = new StoryProcessor(db, mockAnalyzer.Object);

        var storiesToProcess = new List<Story>
        {
            new Story { ExternalId = existingExternalId, Title = "Duplicate" }, // Duplicate
            new Story { ExternalId = "new_456", Title = "New Story", BodyText = "Spooky!", Author = "Writer" } // New
        };

        // Act
        await processor.ProcessAndSaveStoriesAsync(storiesToProcess);

        // Assert
        Assert.Equal(2, await db.Stories.CountAsync()); // 1 original + 1 new
        Assert.Contains(db.Stories, s => s.ExternalId == "new_456");
        Assert.Contains(db.Stories, s => s.ScaryScore == 75);
    }
}
