using Crawler.Services;
using Moq;
using Shared.Models;
using Shared.Data;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Crawler.Tests;

public class CrawlerAppTests
{
    [Fact]
    public async Task RunAsync_ExecutesSuccessfully()
    {
        // Arrange
        var mockReddit = new Mock<IRedditService>();
        var mockYoutube = new Mock<IYouTubeService>();
        var mockProcessor = new Mock<IStoryProcessor>();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "CrawlerAppTestDb")
            .Options;
        var db = new AppDbContext(options);

        mockReddit.Setup(r => r.GetTopStoriesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Story> { new Story { Title = "Reddit Story" } });
        
        mockYoutube.Setup(y => y.GetStoriesFromChannelAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Story> { new Story { Title = "YouTube Story" } });

        var app = new CrawlerApp(mockReddit.Object, mockYoutube.Object, mockProcessor.Object, db);

        // Act
        await app.RunAsync();

        // Assert
        mockReddit.Verify(r => r.GetTopStoriesAsync(It.IsAny<string>()), Times.Exactly(4));
        mockYoutube.Verify(y => y.GetStoriesFromChannelAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(4));
        mockProcessor.Verify(p => p.ProcessAndSaveStoriesAsync(It.IsAny<List<Story>>()), Times.Exactly(8));
    }

    [Fact]
    public async Task RunAsync_Throws_OnException()
    {
        // Arrange
        var mockReddit = new Mock<IRedditService>();
        mockReddit.Setup(r => r.GetTopStoriesAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Fail"));

        var app = new CrawlerApp(mockReddit.Object, null!, null!, null!);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => app.RunAsync());
    }
}
