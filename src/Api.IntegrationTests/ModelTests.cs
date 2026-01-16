using Api.Models;
using Shared.Models;
using Xunit;

namespace Api.IntegrationTests;

public class ModelTests
{
    [Fact]
    public void PagedResult_CanBeInitialized()
    {
        // Act
        var result = new PagedResult<Story>
        {
            Items = new List<Story>(),
            TotalCount = 10,
            Page = 2,
            PageSize = 5
        };

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(2, result.TotalPages); // (10 + 5 - 1) / 5 = 2
    }

    [Fact]
    public void StoryQueryParameters_HasDefaultValues()
    {
        // Act
        var parameters = new StoryQueryParameters();

        // Assert
        Assert.Equal(1, parameters.Page);
        Assert.Equal(50, parameters.PageSize);
        Assert.Null(parameters.SearchTerm);
        Assert.Null(parameters.MinScaryScore);
    }
}
