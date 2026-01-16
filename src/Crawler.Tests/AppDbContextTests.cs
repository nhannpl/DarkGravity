using Shared.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Crawler.Tests;

public class AppDbContextTests
{
    [Fact]
    public void AppDbContext_CanBeInstantiated()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        // Act
        using var context = new AppDbContext(options);

        // Assert
        Assert.NotNull(context);
    }
}
