using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net.Http.Json;
using Xunit;

namespace Api.IntegrationTests;

/// <summary>
/// TtsControllerIntegrationTests - Integration tests for TTS API
/// 
/// PURPOSE: Verifies that the TTS endpoints are correctly wired up
/// and handle requests/responses as expected.
/// </summary>
public class TtsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ITtsService> _ttsServiceMock = new();

    public TtsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the real ITtsService with a mock
                services.Replace(ServiceDescriptor.Scoped<ITtsService>(_ => _ttsServiceMock.Object));
            });
        });
    }

    [Fact]
    public async Task GetVoices_ReturnsVoicesFromMockService()
    {
        // Arrange
        var client = _factory.CreateClient();
        var voices = new List<VoiceInfo>
        {
            new VoiceInfo { Name = "Test Voice", VoiceId = "test-1", LanguageCode = "en-US", Gender = "Neutral" }
        };
        _ttsServiceMock.Setup(s => s.GetVoicesAsync()).ReturnsAsync(voices);

        // Act
        var response = await client.GetAsync("/api/tts/voices");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<VoiceInfo>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Voice", result[0].Name);
    }

    [Fact]
    public async Task Synthesize_ReturnsAudioFile_FromMockService()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SynthesizeRequest
        {
            Text = "Test text",
            VoiceId = "test-1"
        };
        var audioData = new byte[] { 0xFF, 0xD8, 0xFF }; // Fake MP3/JPEG header
        _ttsServiceMock.Setup(s => s.SynthesizeAsync(request.Text, request.VoiceId, null, 1.0))
            .ReturnsAsync(audioData);

        // Act
        var response = await client.PostAsJsonAsync("/api/tts/synthesize", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("audio/mpeg", response.Content.Headers.ContentType?.MediaType);
        var resultData = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(audioData, resultData);
    }
}
