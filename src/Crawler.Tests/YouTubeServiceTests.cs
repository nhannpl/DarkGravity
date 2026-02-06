using Moq;
using Crawler.Services;
using Shared.Models;
using YoutubeExplode.Videos;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Common;
using YoutubeExplode.Channels;

namespace Crawler.Tests;

public class YouTubeServiceTests
{
    private readonly Mock<IYoutubeClientWrapper> _mockYoutube;
    private readonly YouTubeService _service;

    public YouTubeServiceTests()
    {
        _mockYoutube = new Mock<IYoutubeClientWrapper>();
        _service = new YouTubeService(_mockYoutube.Object);
    }

    [Fact]
    public async Task GetStoriesFromChannelAsync_ShouldReturnStories_WhenVideosFound()
    {
        // Arrange
        var query = "horror stories";
        var videoId = new VideoId("abc12345678");

        // Mock Search results (IAsyncEnumerable)
        var searchResults = new List<VideoSearchResult>
        {
            new VideoSearchResult(videoId, "Scary Video", new Author(new ChannelId("chan1"), "Horror Channel"), TimeSpan.FromMinutes(10), new Thumbnail[] { })
        };

        _mockYoutube.Setup(y => y.SearchVideosAsync(query))
            .Returns(searchResults.ToAsyncEnumerable());

        // Mock Video details
        var video = new Video(
            videoId,
            "Scary Video",
            new Author(new ChannelId("chan1"), "Horror Channel"),
            DateTimeOffset.Now,
            "Horror video description",
            TimeSpan.FromMinutes(10),
            new Thumbnail[] { },
            new[] { "horror" },
            new Engagement(1000, 100, 10)
        );

        _mockYoutube.Setup(y => y.GetVideoAsync(videoId))
            .ReturnsAsync(video);

        // Mock transcript failing (to test fallback to description)
        _mockYoutube.Setup(y => y.GetClosedCaptionManifestAsync(videoId))
            .ThrowsAsync(new Exception("No captions"));

        // Act
        var result = await _service.GetStoriesFromChannelAsync(query, 1);

        // Assert
        Assert.Single(result);
        var story = result[0];
        Assert.Equal("Scary Video", story.Title);
        Assert.Equal("Horror Channel", story.Author);
        Assert.Equal("Horror video description", story.BodyText);
        Assert.Equal(1000, story.Upvotes);
    }

    [Fact]
    public async Task GetStoriesFromChannelAsync_ShouldReturnEmpty_WhenExceptionOccurs()
    {
        // Arrange
        _mockYoutube.Setup(y => y.SearchVideosAsync(It.IsAny<string>()))
            .Throws(new Exception("API Error"));

        // Act
        var result = await _service.GetStoriesFromChannelAsync("test", 1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetStoriesFromChannelAsync_ShouldReturnStories_WithTranscriptContent_WhenFound()
    {
        // Arrange
        var query = "horror stories";
        var videoId = new VideoId("xyz98765432");
        var searchResults = new List<VideoSearchResult>
        {
            new VideoSearchResult(videoId, "Transcript Video", new Author(new ChannelId("chan2"), "Horror Channel 2"), TimeSpan.FromMinutes(5), new Thumbnail[] { })
        };
        _mockYoutube.Setup(y => y.SearchVideosAsync(query)).Returns(searchResults.ToAsyncEnumerable());

        var video = new Video(videoId, "Transcript Video", new Author(new ChannelId("chan2"), "Horror Channel 2"), DateTimeOffset.Now, "Desc", TimeSpan.FromMinutes(5), new Thumbnail[] { }, new[] { "horror" }, new Engagement(500, 50, 5));
        _mockYoutube.Setup(y => y.GetVideoAsync(videoId)).ReturnsAsync(video);

        // Mock Manifest
        // Mock Manifest
        var trackInfo = new ClosedCaptionTrackInfo("https://test.com", new Language("en", "English"), false);
        var manifest = new ClosedCaptionManifest(new[] { trackInfo });
        _mockYoutube.Setup(y => y.GetClosedCaptionManifestAsync(videoId)).ReturnsAsync(manifest);

        // Mock Track
        var captions = new[] {
            new ClosedCaption("Caption 1 content.", TimeSpan.Zero, TimeSpan.FromSeconds(1), Array.Empty<ClosedCaptionPart>()),
            new ClosedCaption("Caption 2 content.", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), Array.Empty<ClosedCaptionPart>())
        };
        var track = new ClosedCaptionTrack(captions);
        _mockYoutube.Setup(y => y.GetClosedCaptionTrackAsync(trackInfo)).ReturnsAsync(track);

        // Act
        var result = await _service.GetStoriesFromChannelAsync(query, 1);

        // Assert
        Assert.Single(result);
        Assert.Contains("Caption 1 content.", result[0].BodyText);
        Assert.Contains("Caption 2 content.", result[0].BodyText);
    }
}

// Helper to convert IEnumerable to IAsyncEnumerable for testing
public static class TestExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            yield return item;
            await Task.Yield();
        }
    }
}
