using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// TtsController - Google Cloud Text-to-Speech Integration
/// 
/// PURPOSE: Provides secure backend proxy for TTS synthesis.
/// Uses ITtsService for abstraction and testability.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TtsController : ControllerBase
{
    private readonly ILogger<TtsController> _logger;
    private readonly ITtsService _ttsService;

    public TtsController(ILogger<TtsController> logger, ITtsService ttsService)
    {
        _logger = logger;
        _ttsService = ttsService;
    }

    /// <summary>
    /// GET /api/tts/voices
    /// 
    /// Returns list of available high-quality voices
    /// </summary>
    [HttpGet("voices")]
    [ProducesResponseType(typeof(List<VoiceInfo>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetVoices()
    {
        try
        {
            var voices = await _ttsService.GetVoicesAsync();
            _logger.LogInformation("Fetched {Count} voices from TTS service", voices.Count);
            return Ok(voices);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "TTS configuration issue");
            return StatusCode(500, new { error = "TTS service configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch voices");
            return StatusCode(500, new { error = "Failed to fetch voices", details = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/tts/synthesize
    /// 
    /// Synthesizes text to speech and returns MP3 audio data
    /// </summary>
    [HttpPost("synthesize")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Synthesize([FromBody] SynthesizeRequest request)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest(new { error = "Text is required" });
            }

            if (string.IsNullOrWhiteSpace(request.VoiceId))
            {
                return BadRequest(new { error = "VoiceId is required" });
            }

            _logger.LogInformation(
                "Synthesizing text (length: {Length}) with voice {Voice} at rate {Rate}", 
                request.Text.Length, 
                request.VoiceId,
                request.Rate);

            var audioData = await _ttsService.SynthesizeAsync(
                request.Text,
                request.VoiceId,
                request.LanguageCode,
                request.Rate);

            return File(audioData, "audio/mpeg", "speech.mp3");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "TTS configuration issue during synthesis");
            return StatusCode(500, new { error = "TTS service configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synthesize speech");
            return StatusCode(500, new { error = "Speech synthesis failed", details = ex.Message });
        }
    }
}
