using System.Net.Http.Json;
using Api.Models;
using Shared.Constants;
using Shared.Models;
using Xunit;

namespace Api.IntegrationTests;

public class StoriesControllerTests : BaseIntegrationTest
{
    [Fact]
    public async Task GetStories_ShouldFilterBySearchTerm()
    {
        // Arrange
        var stories = new List<Story>
        {
            new() { Title = "The Ghost in the Attic", BodyText = "A scary ghost story.", Upvotes = 10, ScaryScore = 5 },
            new() { Title = "The Monster under the Bed", BodyText = "A creepy monster tale.", Upvotes = 5, ScaryScore = 8 },
            new() { Title = "Routine Day", BodyText = "Nothing scary happens.", Upvotes = 20, ScaryScore = 1 }
        };
        DbContext.Stories.AddRange(stories);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/stories?searchTerm=Ghost");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Story>>();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("The Ghost in the Attic", result.Items.First().Title);
    }

    [Fact]
    public async Task GetStories_ShouldFilterByMinScaryScore()
    {
        // Arrange
        var stories = new List<Story>
        {
            new() { Title = "Mild Scare", Upvotes = 10, ScaryScore = 3 },
            new() { Title = "Extreme Terror", Upvotes = 5, ScaryScore = 9 },
            new() { Title = "Unsettling Night", Upvotes = 20, ScaryScore = 6 }
        };
        DbContext.Stories.AddRange(stories);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/stories?minScaryScore=5");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Story>>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.All(result.Items, s => Assert.True(s.ScaryScore >= 5));
    }

    [Fact]
    public async Task GetStories_ShouldSortByScaryScoreDescending()
    {
        // Arrange
        var stories = new List<Story>
        {
            new() { Title = "Score 2", ScaryScore = 2, Upvotes = 1 },
            new() { Title = "Score 9", ScaryScore = 9, Upvotes = 1 },
            new() { Title = "Score 5", ScaryScore = 5, Upvotes = 1 }
        };
        DbContext.Stories.AddRange(stories);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/stories?sortBy={StorySortFields.ScaryScore}&sortOrder={SortOrders.Descending}");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Story>>();

        // Assert
        Assert.NotNull(result);
        var items = result.Items.ToList();
        Assert.Equal(9, items[0].ScaryScore);
        Assert.Equal(5, items[1].ScaryScore);
        Assert.Equal(2, items[2].ScaryScore);
    }

    [Fact]
    public async Task GetStories_ShouldHandlePagination()
    {
        // Arrange
        var stories = Enumerable.Range(1, 20).Select(i => new Story 
        { 
            Title = $"Story {i}", 
            Upvotes = i,
            ScaryScore = 5
        });
        DbContext.Stories.AddRange(stories);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/stories?page=2&pageSize=5&sortBy=Upvotes&sortOrder=asc");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<Story>>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result.TotalCount);
        Assert.Equal(5, result.Items.Count());
        Assert.Equal(2, result.Page);
        // Story 1-5 (Page 1), Story 6-10 (Page 2)
        Assert.Equal("Story 6", result.Items.First().Title);
    }

    [Fact]
    public async Task GetStories_ShouldReturnBadRequest_ForInvalidPageSize()
    {
        // Act
        var response = await Client.GetAsync("/api/stories?pageSize=5000");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
