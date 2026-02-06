using System.Net;
using System.Text.Json;
using Crawler.Services;
using Moq;
using Moq.Protected;
using Shared.Models;
using Xunit;

namespace Crawler.Tests;

public class RedditServiceTests
{
    private const string TestSubredditUrl = "https://reddit.com/r/nosleep.json";
    private const string TestId = "123";
    private const string TestTitle = "Scary Story";
    private const string TestAuthor = "GhostWriter";
    private const string TestPermalink = "/r/nosleep/comments/123/scary_story/";
    private const string TestBody = "Long ago...";
    private const int TestUps = 100;

    [Fact]
    public async Task GetTopStoriesAsync_ReturnsStories_OnSuccess()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var jsonResponse = @"
        {
            ""data"": {
                ""children"": [
                    {
                        ""data"": {
                            ""id"": ""123"",
                            ""title"": ""Scary Story"",
                            ""author"": ""GhostWriter"",
                            ""permalink"": ""/r/nosleep/comments/123/scary_story/"",
                            ""selftext"": ""Long ago..."",
                            ""ups"": 100,
                            ""stickied"": false
                        }
                    },
                    {
                        ""data"": {
                            ""id"": ""456"",
                            ""title"": ""Stickied Post"",
                            ""author"": ""Mod"",
                            ""permalink"": ""/r/nosleep/comments/456/stickied/"",
                            ""selftext"": ""I am a sticky"",
                            ""ups"": 10,
                            ""stickied"": true
                        }
                    }
                ]
            }
        }";

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse),
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var redditService = new RedditService(httpClient);

        // Act
        var result = await redditService.GetTopStoriesAsync(TestSubredditUrl);

        // Assert
        Assert.Single(result);
        Assert.Equal("123", result[0].ExternalId);
        Assert.Equal("Scary Story", result[0].Title);
        Assert.Equal("GhostWriter", result[0].Author);
        Assert.Equal("https://reddit.com/r/nosleep/comments/123/scary_story/", result[0].Url);
        Assert.Equal("Long ago...", result[0].BodyText);
        Assert.Equal(100, result[0].Upvotes);
    }

    [Fact]
    public async Task GetTopStoriesAsync_ReturnsEmptyList_OnException()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var redditService = new RedditService(httpClient);

        // Act
        var result = await redditService.GetTopStoriesAsync(TestSubredditUrl);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTopStoriesAsync_HandlesMissingFields_ByUsingDefaults()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var jsonResponse = @"
        {
            ""data"": {
                ""children"": [
                    {
                        ""data"": {
                            ""id"": null,
                            ""title"": null,
                            ""author"": null,
                            ""permalink"": ""/r/test"",
                            ""selftext"": null,
                            ""ups"": 0,
                            ""stickied"": false
                        }
                    }
                ]
            }
        }";

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse),
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var redditService = new RedditService(httpClient);

        // Act
        var result = await redditService.GetTopStoriesAsync(TestSubredditUrl);

        // Assert
        Assert.Single(result);
        Assert.NotNull(result[0].ExternalId); // Should generate a Guid if id is null
        Assert.Equal("Unknown", result[0].Title);
        Assert.Equal("Unknown", result[0].Author);
        Assert.Equal("", result[0].BodyText);
    }

    [Fact]
    public async Task GetTopStoriesAsync_ReturnsEmptyList_OnMalformedJson()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("invalid json { ["),
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var redditService = new RedditService(httpClient);

        // Act
        var result = await redditService.GetTopStoriesAsync(TestSubredditUrl);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTopStoriesAsync_ReturnsEmptyList_OnEmptyData()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var jsonResponse = @"{ ""data"": { ""children"": [] } }";

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse),
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var redditService = new RedditService(httpClient);

        // Act
        var result = await redditService.GetTopStoriesAsync(TestSubredditUrl);

        // Assert
        Assert.Empty(result);
    }
}
