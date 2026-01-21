using Google.Cloud.TextToSpeech.V1;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Api.Controllers;

/// <summary>
/// TtsController - Google Cloud Text-to-Speech Integration
/// 
/// PURPOSE: Provides secure backend proxy for TTS synthesis
/// 
/// ARCHITECTURE:
/// - Frontend calls this API (no API key exposure)
/// - This calls Google Cloud TTS with server-side credentials
/// - Returns audio blob to frontend
/// 
/// RESUME VALUE:
/// - Shows GCP integration
/// - Demonstrates API security best practices
/// - Full-stack architecture
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TtsController : ControllerBase
{
    private readonly ILogger<TtsController> _logger;
    private readonly IConfiguration _configuration;
    private TextToSpeechClient? _ttsClient;

    public TtsController(ILogger<TtsController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Get TTS client - lazy initialization with API key from configuration
    /// </summary>
    private TextToSpeechClient GetClient()
    {
        if (_ttsClient != null) return _ttsClient;

        var apiKey = _configuration["GoogleCloud:TtsApiKey"];
        
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Google Cloud TTS API key not configured");
            throw new InvalidOperationException(
                "Google Cloud TTS API key not found. " +
                "Please add 'GoogleCloud:TtsApiKey' to user secrets or appsettings.json");
        }

        // Create client builder with API key
        var clientBuilder = new TextToSpeechClientBuilder
        {
            ApiKey = apiKey
        };

        _ttsClient = clientBuilder.Build();
        _logger.LogInformation("Google Cloud TTS client initialized");
        
        return _ttsClient;
    }

    /// <summary>
    /// GET /api/tts/voices
    /// 
    /// Returns list of available Google Cloud TTS voices
    /// Filters to English neural voices for simplicity
    /// </summary>
    [HttpGet("voices")]
    [ProducesResponseType(typeof(List<VoiceInfo>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetVoices()
    {
        try
        {
            var client = GetClient();
            var response = await client.ListVoicesAsync(new ListVoicesRequest
            {
                LanguageCode = "en-US" // English voices only
            });

            // Map to simplified structure for frontend
            var voices = response.Voices
                .Where(v => v.Name.Contains("Neural") || v.Name.Contains("Wavenet")) // High-quality voices only
                .Select(v => new VoiceInfo
                {
                    Name = $"{v.Name} ({v.SsmlGender})",
                    VoiceId = v.Name,
                    LanguageCode = v.LanguageCodes.FirstOrDefault() ?? "en-US",
                    Gender = v.SsmlGender.ToString()
                })
                .ToList();

            _logger.LogInformation("Fetched {Count} voices from Google Cloud TTS", voices.Count);
            
            return Ok(voices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch voices from Google Cloud TTS");
            return StatusCode(500, new { error = "Failed to fetch voices", details = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/tts/synthesize
    /// 
    /// Synthesizes text to speech using Google Cloud TTS
    /// Returns MP3 audio data
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

            var client = GetClient();

            // Build synthesis request
            var synthesisInput = new SynthesisInput
            {
                Text = request.Text
            };

            var voice = new VoiceSelectionParams
            {
                Name = request.VoiceId,
                LanguageCode = request.LanguageCode ?? "en-US"
            };

            // Calculate speaking rate (Google accepts 0.25 to 4.0)
            var speakingRate = Math.Clamp(request.Rate, 0.25, 4.0);

            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3,
                SpeakingRate = speakingRate
            };

            _logger.LogInformation(
                "Synthesizing text (length: {Length}) with voice {Voice} at rate {Rate}", 
                request.Text.Length, 
                request.VoiceId, 
                speakingRate);

            // Call Google Cloud TTS
            var response = await client.SynthesizeSpeechAsync(synthesisInput, voice, audioConfig);

            // Return audio as MP3
            return File(response.AudioContent.ToByteArray(), "audio/mpeg", "speech.mp3");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synthesize speech");
            return StatusCode(500, new { error = "Speech synthesis failed", details = ex.Message });
        }
    }
}

/// <summary>
/// Request model for text synthesis
/// </summary>
public record SynthesizeRequest
{
    /// <summary>Text to synthesize (max ~5000 characters)</summary>
    public required string Text { get; init; }
    
    /// <summary>Voice ID (e.g., "en-US-Neural2-C")</summary>
    public required string VoiceId { get; init; }
    
    /// <summary>Language code (default: "en-US")</summary>
    public string? LanguageCode { get; init; }
    
    /// <summary>Speaking rate (0.25 to 4.0, default: 1.0)</summary>
    public double Rate { get; init; } = 1.0;
}

/// <summary>
/// Voice information for frontend
/// </summary>
public record VoiceInfo
{
    /// <summary>Display name</summary>
    public required string Name { get; init; }
    
    /// <summary>Voice identifier for synthesis</summary>
    public required string VoiceId { get; init; }
    
    /// <summary>Language code (e.g., "en-US")</summary>
    public required string LanguageCode { get; init; }
    
    /// <summary>Voice gender (Male, Female, Neutral)</summary>
    public required string Gender { get; init; }
}
