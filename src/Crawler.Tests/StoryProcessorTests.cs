using Moq;
using Crawler.Services;
using Shared.Models;
using Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace Crawler.Tests;

public class StoryProcessorTests
{
    private readonly AppDbContext _db;
    private readonly Mock<IStoryAnalyzer> _mockAnalyzer;
    private readonly StoryProcessor _processor;

    public StoryProcessorTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        _mockAnalyzer = new Mock<IStoryAnalyzer>();
        _processor = new StoryProcessor(_db, _mockAnalyzer.Object);
    }

    [Fact]
    public async Task ProcessAndSaveStoriesAsync_ShouldSaveNewStories()
    {
        // Arrange
        var stories = new List<Story>
        {
            new Story { ExternalId = "s1", Title = "Story 1", BodyText = "Text 1" },
            new Story { ExternalId = "s2", Title = "Story 2", BodyText = "Text 2" }
        };

        _mockAnalyzer.Setup(a => a.AnalyzeAsync(It.IsAny<Story>()))
            .ReturnsAsync(("AI Analysis", 7.5));

        // Act
        await _processor.ProcessAndSaveStoriesAsync(stories);

        // Assert
        Assert.Equal(2, await _db.Stories.CountAsync());
        var savedStory = await _db.Stories.FirstAsync(s => s.ExternalId == "s1");
        Assert.Equal("AI Analysis", savedStory.AiAnalysis);
        Assert.Equal(7.5, savedStory.ScaryScore);
    }

    [Fact]
    public async Task ProcessAndSaveStoriesAsync_ShouldSkipExistingStories()
    {
        // Arrange
        _db.Stories.Add(new Story { ExternalId = "existing", Title = "Old", BodyText = "Old Text" });
        await _db.SaveChangesAsync();

        var stories = new List<Story>
        {
            new Story { ExternalId = "existing", Title = "New", BodyText = "New Text" },
            new Story { ExternalId = "unique", Title = "Unique", BodyText = "Unique Text" }
        };

        _mockAnalyzer.Setup(a => a.AnalyzeAsync(It.IsAny<Story>()))
            .ReturnsAsync(("AI Analysis", 5.0));

        // Act
        await _processor.ProcessAndSaveStoriesAsync(stories);

        // Assert
        Assert.Equal(2, await _db.Stories.CountAsync());
        Assert.True(await _db.Stories.AnyAsync(s => s.ExternalId == "existing" && s.Title == "Old"));
        Assert.True(await _db.Stories.AnyAsync(s => s.ExternalId == "unique"));
    }
}
