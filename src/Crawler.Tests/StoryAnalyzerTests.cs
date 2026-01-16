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
        _config.Setup(c => c["GEMINI_API_KEY"]).Returns((string?)null); // Force skip Gemini
        _config.Setup(c => c["DEEPSEEK_API_KEY"]).Returns("fake_key");
        var analyzer = new StoryAnalyzer(_client, _config.Object);
        var story = new Story { Title = "T", BodyText = "B" };

        var deepseekResponse = $@"{{ ""choices"": [ {{ ""message"": {{ ""content"": ""{aiResponse}"" }} }} ] }}";

        _handler.SetupRequest(HttpMethod.Post, r => r.RequestUri!.ToString().Contains("deepseek"))
                .ReturnsResponse(deepseekResponse, "application/json");

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Equal(expectedScore, result.Score);
    }

    [Fact]
    public async Task AnalyzeAsync_UsesCloudflare_WhenTokensPresent()
    {
        // Arrange
        _config.Setup(c => c["GEMINI_API_KEY"]).Returns((string?)null);
        _config.Setup(c => c["DEEPSEEK_API_KEY"]).Returns((string?)null);
        _config.Setup(c => c["CLOUDF_API_TOKEN"]).Returns("fake_token");
        _config.Setup(c => c["CLOUDFLARE_ACCOUNT_ID"]).Returns("fake_id");

        var analyzer = new StoryAnalyzer(_client, _config.Object);
        var story = new Story { Title = "T", BodyText = "B" };

        var cfResponse = @"{ ""result"": { ""response"": ""Cloudflare Analysis. Score: 9"" } }";

        _handler.SetupRequest(HttpMethod.Post, r => r.RequestUri!.ToString().Contains("cloudflare"))
                .ReturnsResponse(cfResponse, "application/json");

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Equal(9.0, result.Score);
        Assert.Contains("Cloudflare", result.Analysis);
    }

    [Fact]
    public async Task AnalyzeAsync_UsesHuggingFace_WhenKeyPresent()
    {
        // Arrange
        _config.Setup(c => c["GEMINI_API_KEY"]).Returns((string?)null);
        _config.Setup(c => c["DEEPSEEK_API_KEY"]).Returns((string?)null);
        _config.Setup(c => c["CLOUDF_API_TOKEN"]).Returns((string?)null);
        _config.Setup(c => c["HUGGINGFACE_API_KEY"]).Returns("fake_hf_key");

        var analyzer = new StoryAnalyzer(_client, _config.Object);
        var story = new Story { Title = "T", BodyText = "B" };

        var hfResponse = @"{ ""generated_text"": ""HF Analysis. Score: 7"" }";

        _handler.SetupRequest(HttpMethod.Post, r => r.RequestUri!.ToString().Contains("huggingface"))
                .ReturnsResponse(hfResponse, "application/json");

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Equal(7.0, result.Score);
        Assert.Contains("HF", result.Analysis);
    }

    [Fact]
    public async Task AnalyzeAsync_UsesHuggingFace_ArrayResponse()
    {
        // Arrange
        _config.Setup(c => c["HUGGINGFACE_API_KEY"]).Returns("fake_hf_key");
        var analyzer = new StoryAnalyzer(_client, _config.Object);
        var story = new Story { Title = "T", BodyText = "B" };

        var hfResponse = @"[{ ""generated_text"": ""HF Array Analysis. Score: 6"" }]";

        _handler.SetupRequest(HttpMethod.Post, r => r.RequestUri!.ToString().Contains("huggingface"))
                .ReturnsResponse(hfResponse, "application/json");

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Equal(6.0, result.Score);
    }

    [Fact]
    public async Task AnalyzeAsync_Failover_WhenFirstProviderExceedsQuota()
    {
        // Arrange
        _config.Setup(c => c["GEMINI_API_KEY"]).Returns("fake_gemini");
        _config.Setup(c => c["DEEPSEEK_API_KEY"]).Returns("fake_deepseek");

        var analyzer = new StoryAnalyzer(_client, _config.Object);
        var story = new Story { Title = "T", BodyText = "B" };

        // Gemini returns 429
        _handler.SetupRequest(HttpMethod.Post, r => r.RequestUri!.ToString().Contains("gemini"))
                .ReturnsResponse(System.Net.HttpStatusCode.TooManyRequests, "Quota Exceeded");

        // DeepSeek returns success
        var dsResponse = @"{ ""choices"": [ { ""message"": { ""content"": ""DeepSeek Fallback. Score: 5"" } } ] }";
        _handler.SetupRequest(HttpMethod.Post, r => r.RequestUri!.ToString().Contains("deepseek"))
                .ReturnsResponse(dsResponse, "application/json");

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Equal(5.0, result.Score);
        Assert.Contains("DeepSeek", result.Analysis);
    }

    [Fact]
    public async Task AnalyzeAsync_FallbackToMock_WhenProviderFailsWithOtherError()
    {
        // Arrange
        _config.Setup(c => c["GEMINI_API_KEY"]).Returns("fake_gemini");
        var analyzer = new StoryAnalyzer(_client, _config.Object);
        var story = new Story { Title = "T", BodyText = "B" };

        // Gemini returns 500 (not a quota issue, but still causes a failover in my code logic if it's the only one 
        // OR returns Error: 500 which triggers the mock fallback check)
        _handler.SetupRequest(HttpMethod.Post, r => r.RequestUri!.ToString().Contains("gemini"))
                .ReturnsResponse(System.Net.HttpStatusCode.InternalServerError, "Error");

        // Act
        var result = await analyzer.AnalyzeAsync(story);

        // Assert
        Assert.Contains("MOCK ANALYSIS", result.Analysis);
    }
}
