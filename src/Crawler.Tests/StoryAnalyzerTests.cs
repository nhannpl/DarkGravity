using Shared.Models;
using Crawler.Services;
using Moq;
using Moq.Contrib.HttpClient;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace Crawler.Tests;

public class StoryAnalyzerTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _handler;
    private readonly HttpClient _client;
    private readonly Mock<IConfiguration> _config;

    public StoryAnalyzerTests()
    {
        _handler = new Mock<HttpMessageHandler>();
        _client = _handler.CreateClient();
        _config = new Mock<IConfiguration>();
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
        _config.Setup(c => c["GEMINI_API_KEY"]).Returns("fake_key");

        var analyzer = new StoryAnalyzer(_client, _config.Object);
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

        _handler.SetupRequest(HttpMethod.Post, r => r.RequestUri!.ToString().Contains("gemini"))
                .ReturnsResponse(geminiResponse, "application/json");

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Equal("Ghost Story. Score: 8/10", result.Analysis);
    }

    [Fact]
    public async Task AnalyzeAsync_FallsBack_WhenNoKeysPresent()
    {
        // Arrange
        _config.Setup(c => c["GEMINI_API_KEY"]).Returns((string?)null);

        var analyzer = new StoryAnalyzer(_client, _config.Object);
        var story = new Story { Title = "Test", BodyText = "Body" };

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Contains("MOCK ANALYSIS", result.Analysis);
    }

    [Theory]
    [InlineData("This is scary. Score: 8.5/10", 8.5)]
    [InlineData("Spooky! score: 7", 7.0)]
    [InlineData("Terrifying! 9.2/10", 9.2)]
    [InlineData("No score here.", null)]
    public async Task AnalyzeAsync_CorrectlyParsesVariousScoreFormats(string aiResponse, double? expectedScore)
    {
        // Arrange
        _config.Setup(c => c["GEMINI_API_KEY"]).Returns("fake_key");
        var analyzer = new StoryAnalyzer(_client, _config.Object);
        var story = new Story { Title = "T", BodyText = "B" };

        var geminiResponse = $@"{{ ""candidates"": [ {{ ""content"": {{ ""parts"": [ {{ ""text"": ""{aiResponse}"" }} ] }} }} ] }}";

        _handler.SetupRequest(HttpMethod.Post, r => r.RequestUri!.ToString().Contains("gemini"))
                .ReturnsResponse(geminiResponse, "application/json");

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Equal(expectedScore, result.Score);
    }
}
