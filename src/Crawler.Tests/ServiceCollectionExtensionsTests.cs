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

        // Mock connection string
        mockConfig.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"]).Returns("Server=localhost;Database=DarkGravity;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;");

        // Mock Kafka settings
        mockConfig.Setup(c => c["KafkaBootstrapServers"]).Returns("localhost:9092");
        mockConfig.Setup(c => c["KafkaTopicStoryFetched"]).Returns("story-fetched");

        // Act
        services.AddCrawlerServices(mockConfig.Object);

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(IYoutubeClientWrapper));
        Assert.Contains(services, s => s.ServiceType == typeof(IRedditService));
        Assert.Contains(services, s => s.ServiceType == typeof(IYouTubeService));
        Assert.Contains(services, s => s.ServiceType == typeof(IStoryProcessor));
    }
}
