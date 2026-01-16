using Api.Controllers;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;
using Shared.Data;
using Shared.Models;
using Xunit;

namespace Api.IntegrationTests;

public class StoriesControllerTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public StoriesControllerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private async Task SeedDatabase(AppDbContext db)
    {
        db.Stories.AddRange(new List<Story>
        {
            new Story { Id = Guid.NewGuid(), Title = "A Scary Story", BodyText = "Boo!", ScaryScore = 8.5, Upvotes = 100, FetchedAt = DateTime.UtcNow.AddHours(-2), ExternalId = "1", Author = "A1", Url = "U1" },
            new Story { Id = Guid.NewGuid(), Title = "Ghost in the Shell", BodyText = "Creepy.", ScaryScore = 4.2, Upvotes = 50, FetchedAt = DateTime.UtcNow.AddHours(-1), ExternalId = "2", Author = "A2", Url = "U2" },
            new Story { Id = Guid.NewGuid(), Title = "Slasher Night", BodyText = "Run!", ScaryScore = 9.8, Upvotes = 200, FetchedAt = DateTime.UtcNow, ExternalId = "3", Author = "A3", Url = "U3" }
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetStories_ReturnsAllStories_WithDefaultParams()
    {
        // Arrange
        using var db = new AppDbContext(_dbOptions);
        await SeedDatabase(db);
        var controller = new StoriesController(db);
        var queryParams = new StoryQueryParameters();

        // Act
        var result = await controller.GetStories(queryParams);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PagedResult<Story>>>(result);
        var pagedResult = Assert.IsType<PagedResult<Story>>(actionResult.Value);
        Assert.Equal(3, pagedResult.TotalCount);
        Assert.Equal(3, pagedResult.Items.Count());
    }

    [Fact]
    public async Task GetStories_FiltersBySearchTerm()
    {
        // Arrange
        using var db = new AppDbContext(_dbOptions);
        await SeedDatabase(db);
        var controller = new StoriesController(db);
        var queryParams = new StoryQueryParameters { SearchTerm = "Slasher" };

        // Act
        var result = await controller.GetStories(queryParams);

        // Assert
        var pagedResult = result.Value;
        Assert.Single(pagedResult.Items);
        Assert.Equal("Slasher Night", pagedResult.Items.First().Title);
    }

    [Fact]
    public async Task GetStories_FiltersByMinScaryScore()
    {
        // Arrange
        using var db = new AppDbContext(_dbOptions);
        await SeedDatabase(db);
        var controller = new StoriesController(db);
        var queryParams = new StoryQueryParameters { MinScaryScore = 9.0 };

        // Act
        var result = await controller.GetStories(queryParams);

        // Assert
        var pagedResult = result.Value;
        Assert.Single(pagedResult.Items);
        Assert.Equal("Slasher Night", pagedResult.Items.First().Title);
    }

    [Fact]
    public async Task GetStories_SortsByScaryScoreDescending()
    {
        // Arrange
        using var db = new AppDbContext(_dbOptions);
        await SeedDatabase(db);
        var controller = new StoriesController(db);
        var queryParams = new StoryQueryParameters 
        { 
            SortBy = StorySortFields.ScaryScore, 
            SortOrder = SortOrders.Descending 
        };

        // Act
        var result = await controller.GetStories(queryParams);

        // Assert
        var items = result.Value.Items.ToList();
        Assert.Equal("Slasher Night", items[0].Title); // 9.8
        Assert.Equal("A Scary Story", items[1].Title); // 8.5
        Assert.Equal("Ghost in the Shell", items[2].Title); // 4.2
    }

    [Fact]
    public async Task GetStory_ReturnsNotFound_WhenIdMissing()
    {
        // Arrange
        using var db = new AppDbContext(_dbOptions);
        var controller = new StoriesController(db);

        // Act
        var result = await controller.GetStory(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetStory_ReturnsStory_WhenIdExists()
    {
        // Arrange
        using var db = new AppDbContext(_dbOptions);
        var id = Guid.NewGuid();
        db.Stories.Add(new Story { Id = id, Title = "Found", BodyText = "Text", Author = "A", Url = "U", ExternalId = "E" });
        await db.SaveChangesAsync();
        var controller = new StoriesController(db);

        // Act
        var result = await controller.GetStory(id);

        // Assert
        Assert.Equal("Found", result.Value.Title);
    }
}
