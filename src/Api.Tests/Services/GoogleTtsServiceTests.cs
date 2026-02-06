using Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Api.Tests.Services;

public class GoogleTtsServiceTests
{
    private readonly Mock<ILogger<GoogleTtsService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public GoogleTtsServiceTests()
    {
        _loggerMock = new Mock<ILogger<GoogleTtsService>>();
        _configurationMock = new Mock<IConfiguration>();
    }

    [Fact]
    public async Task GetVoicesAsync_ThrowsInvalidOperationException_WhenApiKeyMissing()
    {
        // Arrange
        _configurationMock.Setup(c => c["GoogleCloud:TtsApiKey"]).Returns((string?)null);
        var service = new GoogleTtsService(_loggerMock.Object, _configurationMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetVoicesAsync());
        Assert.Contains("Google Cloud TTS API key not found", ex.Message);
    }

    [Fact]
    public async Task SynthesizeAsync_ThrowsInvalidOperationException_WhenApiKeyMissing()
    {
        // Arrange
        _configurationMock.Setup(c => c["GoogleCloud:TtsApiKey"]).Returns((string?)null);
        var service = new GoogleTtsService(_loggerMock.Object, _configurationMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.SynthesizeAsync("text", "voice"));
        Assert.Contains("Google Cloud TTS API key not found", ex.Message);
    }
}
