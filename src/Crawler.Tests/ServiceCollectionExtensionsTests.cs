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
        services.AddSingleton<IConfiguration>(mockConfig.Object);

        // Act
        services.AddCrawlerServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IYoutubeClientWrapper>());
        Assert.NotNull(serviceProvider.GetService<IRedditService>());
        Assert.NotNull(serviceProvider.GetService<IYouTubeService>());
        Assert.NotNull(serviceProvider.GetService<IStoryAnalyzer>());
        Assert.NotNull(serviceProvider.GetService<IStoryProcessor>());
    }
}
