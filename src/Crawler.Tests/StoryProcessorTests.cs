using Moq;
using Crawler.Services;
using Shared.Models;
using Shared.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using DarkGravity.Contracts.Events;

namespace Crawler.Tests;

public class StoryProcessorTests
{
    private readonly AppDbContext _db;
    private readonly Mock<ITopicProducer<StoryFetched>> _mockProducer;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly StoryProcessor _processor;

    public StoryProcessorTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _mockProducer = new Mock<ITopicProducer<StoryFetched>>();

        // Mock IServiceProvider to return the mocked producer when requested
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ITopicProducer<StoryFetched>)))
            .Returns(_mockProducer.Object);

        _processor = new StoryProcessor(_db, _mockServiceProvider.Object);
    }

    [Fact]
    public async Task ProcessAndSaveStoriesAsync_ShouldSaveNewStories_WithEmptyAnalysis()
    {
        // Arrange
        var stories = new List<Story>
        {
            new Story { ExternalId = "s1", Title = "Story 1", BodyText = "Text 1" },
            new Story { ExternalId = "s2", Title = "Story 2", BodyText = "Text 2" }
        };

        // Act
        await _processor.ProcessAndSaveStoriesAsync(stories);

        // Assert
        Assert.Equal(2, await _db.Stories.CountAsync());
        var savedStory = await _db.Stories.FirstAsync(s => s.ExternalId == "s1");
        
        // Now Crawler saves it as empty, Analyzer picks it up later
        Assert.Equal(string.Empty, savedStory.AiAnalysis);
        Assert.Null(savedStory.ScaryScore);
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

        // Act
        await _processor.ProcessAndSaveStoriesAsync(stories);

        // Assert
        Assert.Equal(2, await _db.Stories.CountAsync());
        Assert.True(await _db.Stories.AnyAsync(s => s.ExternalId == "existing" && s.Title == "Old"));
        Assert.True(await _db.Stories.AnyAsync(s => s.ExternalId == "unique"));
    }
}
