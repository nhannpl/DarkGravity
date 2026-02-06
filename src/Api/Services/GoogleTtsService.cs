using Google.Cloud.TextToSpeech.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Api.Services;

/// <summary>
/// GoogleTtsService - Google Cloud Text-to-Speech implementation
/// </summary>
public class GoogleTtsService : ITtsService
{
    private readonly ILogger<GoogleTtsService> _logger;
    private readonly IConfiguration _configuration;
    private TextToSpeechClient? _ttsClient;

    public GoogleTtsService(ILogger<GoogleTtsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Lazy initialization of the TTS client
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

        var clientBuilder = new TextToSpeechClientBuilder
        {
            ApiKey = apiKey
        };

        _ttsClient = clientBuilder.Build();
        _logger.LogInformation("Google Cloud TTS client initialized");
        
        return _ttsClient;
    }

    /// <inheritdoc />
    public async Task<List<VoiceInfo>> GetVoicesAsync()
    {
        try
        {
            var client = GetClient();
            var response = await client.ListVoicesAsync(new ListVoicesRequest
            {
                LanguageCode = "en-US"
            });

            return response.Voices
                .Where(v => v.Name.Contains("Neural") || v.Name.Contains("Wavenet"))
                .Select(v => new VoiceInfo
                {
                    Name = $"{v.Name} ({v.SsmlGender})",
                    VoiceId = v.Name,
                    LanguageCode = v.LanguageCodes.FirstOrDefault() ?? "en-US",
                    Gender = v.SsmlGender.ToString()
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch voices from Google Cloud TTS");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> SynthesizeAsync(string text, string voiceId, string? languageCode = null, double rate = 1.0)
    {
        try
        {
            var client = GetClient();

            var synthesisInput = new SynthesisInput { Text = text };
            var voice = new VoiceSelectionParams
            {
                Name = voiceId,
                LanguageCode = languageCode ?? "en-US"
            };

            var speakingRate = Math.Clamp(rate, 0.25, 4.0);

            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3,
                SpeakingRate = speakingRate
            };

            var response = await client.SynthesizeSpeechAsync(synthesisInput, voice, audioConfig);
            return response.AudioContent.ToByteArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synthesize speech using Google Cloud TTS");
            throw;
        }
    }
}
