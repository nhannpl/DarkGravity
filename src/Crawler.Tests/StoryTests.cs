using Shared.Models;
using Xunit;

namespace Crawler.Tests;

public class StoryTests
{
    [Fact]
    public void Story_Properties_SetCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var story = new Story
        {
            Id = id,
            ExternalId = "ext_1",
            Title = "Title",
            Author = "Author",
            Url = "url",
            BodyText = "Body",
            AiAnalysis = "Analysis",
            ScaryScore = 8.5,
            Upvotes = 100,
            FetchedAt = now
        };

        // Assert
        Assert.Equal(id, story.Id);
        Assert.Equal("ext_1", story.ExternalId);
        Assert.Equal("Title", story.Title);
        Assert.Equal("Author", story.Author);
        Assert.Equal("url", story.Url);
        Assert.Equal("Body", story.BodyText);
        Assert.Equal("Analysis", story.AiAnalysis);
        Assert.Equal(8.5, story.ScaryScore);
        Assert.Equal(100, story.Upvotes);
        Assert.Equal(now, story.FetchedAt);
    }
}
