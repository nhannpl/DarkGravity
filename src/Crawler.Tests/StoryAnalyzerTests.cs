using Crawler.Models;
using Crawler.Services;
using Moq;
using Moq.Contrib.HttpClient;
using Xunit;

namespace Crawler.Tests;

public class StoryAnalyzerTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _handler;
    private readonly HttpClient _client;

    public StoryAnalyzerTests()
    {
        _handler = new Mock<HttpMessageHandler>();
        _client = _handler.CreateClient();
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", null);
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);
    }

    [Fact]
    public async Task AnalyzeAsync_UsesGemini_WhenKeyIsPresent()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", "fake_key");
        var analyzer = new StoryAnalyzer(_client);
        var story = new Story { Title = "Test Title", BodyText = "This is a scary body text." };

        // Mock Gemini JSON Response
        var geminiResponse = """
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  { "text": "Ghost Story. Score: 8/10" }
                ]
              }
            }
          ]
        }
        """;

        _handler.SetupRequest(HttpMethod.Post, r => r.RequestUri.ToString().Contains("gemini")) 
                .ReturnsResponse(geminiResponse, "application/json");

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Equal("Ghost Story. Score: 8/10", result);
    }

    [Fact]
    public async Task AnalyzeAsync_FallsBack_WhenNoKeysPresent()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", null);
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);
        
        var analyzer = new StoryAnalyzer(_client);
        var story = new Story { Title = "Test", BodyText = "Body" };

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Contains("MOCK ANALYSIS", result);
    }
}
