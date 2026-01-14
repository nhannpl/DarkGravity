using Moq;
using Moq.Contrib.HttpClient;
using Xunit;
using Crawler;

namespace Crawler.Tests;

public class RedditServiceTests
{
    private readonly Mock<HttpMessageHandler> _handler;
    private readonly HttpClient _client;

    public RedditServiceTests()
    {
        _handler = new Mock<HttpMessageHandler>();
        _client = _handler.CreateClient();
    }

    [Fact]
    public async Task GetTopStories_ReturnsStories_WhenJsonIsValid()
    {
        // Arrange
        var json = """
        {
            "data": {
                "children": [
                    { 
                        "data": { 
                            "id": "1", "title": "Scary Story 1", "author": "User1", 
                            "permalink": "/r/test/1", "selftext": "Boo", "ups": 100, 
                            "stickied": false 
                        } 
                    },
                    { 
                        "data": { 
                            "id": "2", "title": "Announcement", "author": "Mod", 
                            "permalink": "/r/test/2", "selftext": "Rules", "ups": 999, 
                            "stickied": true 
                        } 
                    }
                ]
            }
        }
        """;

        _handler.SetupRequest(HttpMethod.Get, "https://api.reddit.com/stories")
                .ReturnsResponse(json, "application/json");

        var service = new RedditService(_client);

        // Act
        var result = await service.GetTopStoriesAsync("https://api.reddit.com/stories");

        // Assert
        Assert.Single(result); // Should skip the stickied one
        Assert.Equal("Scary Story 1", result[0].Title);
        Assert.Equal("User1", result[0].Author);
        Assert.Equal(100, result[0].Upvotes);
    }

    [Fact]
    public async Task GetTopStories_ReturnsEmpty_WhenApiFails()
    {
        _handler.SetupRequest(HttpMethod.Get, "https://api.reddit.com/fail")
                .ReturnsResponse(System.Net.HttpStatusCode.InternalServerError);

        var service = new RedditService(_client);

        var result = await service.GetTopStoriesAsync("https://api.reddit.com/fail");

        Assert.Empty(result);
    }
}
