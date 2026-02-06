using System.Net.Http.Json;
using Api.Models;
using Shared.Models;
using Xunit;

namespace Api.IntegrationTests;

/// <summary>
/// Integration tests for StoriesController using a real SQL Server container.
/// Verifies that the full stack (API -> SQL) works as expected.
/// </summary>
public class RealStoriesControllerTests : BaseIntegrationTest
{
    private const string TestExternalId = "real-sql-test-1";
    private const string TestTitle = "Real Horror Story";

    [Fact]
    public async Task GetStories_ReturnsItemsFromRealDatabase()
    {
        // Arrange
        var testStory = new Story 
        { 
            Id = Guid.NewGuid(),
            ExternalId = TestExternalId, 
            Title = TestTitle, 
            BodyText = "This story is stored in a real MS SQL container.",
            Author = "IntegrationTest",
            Url = "https://reddit.com/r/test",
            FetchedAt = DateTime.UtcNow,
            Upvotes = 666
        };
        DbContext.Stories.Add(testStory);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await Client.GetFromJsonAsync<PagedResult<Story>>("/api/stories");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Contains(result.Items, s => s.ExternalId == TestExternalId);
    }

    [Fact]
    public async Task GetStory_ReturnsCorrectStoryFromRealDatabase()
    {
        // Arrange
        var id = Guid.NewGuid();
        var testStory = new Story 
        { 
            Id = id,
            ExternalId = "real-sql-test-2", 
            Title = "Single Story Test", 
            BodyText = "Body",
            Author = "Tester",
            Url = "https://youtube.com/watch?v=1",
            FetchedAt = DateTime.UtcNow
        };
        DbContext.Stories.Add(testStory);
        await DbContext.SaveChangesAsync();

        // Act
        var story = await Client.GetFromJsonAsync<Story>($"/api/stories/{id}");

        // Assert
        Assert.NotNull(story);
        Assert.Equal(id, story.Id);
        Assert.Equal("Single Story Test", story.Title);
    }

    [Fact]
    public async Task GetStory_ReturnsNotFound_WhenIdDoesNotExist()
    {
        // Act
        var response = await Client.GetAsync($"/api/stories/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
