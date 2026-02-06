using Api.Controllers;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Api.Tests.Controllers;

public class TtsControllerTests
{
    private readonly Mock<ITtsService> _ttsServiceMock;
    private readonly Mock<ILogger<TtsController>> _loggerMock;
    private readonly TtsController _controller;

    public TtsControllerTests()
    {
        _ttsServiceMock = new Mock<ITtsService>();
        _loggerMock = new Mock<ILogger<TtsController>>();
        _controller = new TtsController(_loggerMock.Object, _ttsServiceMock.Object);
    }

    [Fact]
    public async Task GetVoices_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var voices = new List<VoiceInfo>
        {
            new VoiceInfo { Name = "Voice 1", VoiceId = "v1", LanguageCode = "en-US", Gender = "Neutral" }
        };
        _ttsServiceMock.Setup(s => s.GetVoicesAsync()).ReturnsAsync(voices);

        // Act
        var result = await _controller.GetVoices();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVoices = Assert.IsType<List<VoiceInfo>>(okResult.Value);
        Assert.Single(returnedVoices);
    }

    [Fact]
    public async Task GetVoices_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _ttsServiceMock.Setup(s => s.GetVoicesAsync()).ThrowsAsync(new Exception("API Error"));

        // Act
        var result = await _controller.GetVoices();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Synthesize_ReturnsFile_WhenSuccessful()
    {
        // Arrange
        var request = new SynthesizeRequest
        {
            Text = "Hello",
            VoiceId = "v1"
        };
        var audioData = new byte[] { 1, 2, 3 };
        _ttsServiceMock.Setup(s => s.SynthesizeAsync(request.Text, request.VoiceId, null, 1.0))
            .ReturnsAsync(audioData);

        // Act
        var result = await _controller.Synthesize(request);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("audio/mpeg", fileResult.ContentType);
        Assert.Equal(audioData, fileResult.FileContents);
    }

    [Fact]
    public async Task Synthesize_ReturnsBadRequest_WhenTextIsEmpty()
    {
        // Arrange
        var request = new SynthesizeRequest { Text = "", VoiceId = "v1" };

        // Act
        var result = await _controller.Synthesize(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
