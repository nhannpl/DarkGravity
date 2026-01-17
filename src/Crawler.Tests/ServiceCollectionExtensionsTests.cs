using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Crawler.Services;
using Moq;
using Xunit;

namespace Crawler.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCrawlerServices_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockConfig = new Mock<IConfiguration>();

        // Mock connection string as it's required now
        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.Value).Returns("Server=test;Database=test;");
        mockConfig.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"]).Returns("Server=test;Database=test;");

        // Act
        services.AddCrawlerServices(mockConfig.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IYoutubeClientWrapper>());
        Assert.NotNull(serviceProvider.GetService<IRedditService>());
        Assert.NotNull(serviceProvider.GetService<IYouTubeService>());

        Assert.NotNull(serviceProvider.GetService<IStoryProcessor>());
    }
}
