using Google.Cloud.TextToSpeech.V1;

namespace Api.Services;

/// <summary>
/// ITtsService - Interface for Text-to-Speech operations
/// 
/// PURPOSE: Decouples the controller from specific TTS implementations (GCP, Azure, etc.)
/// </summary>
public interface ITtsService
{
    /// <summary>
    /// Fetches available voices from the provider
    /// </summary>
    /// <returns>List of voice information</returns>
    Task<List<VoiceInfo>> GetVoicesAsync();

    /// <summary>
    /// Synthesizes text to speech
    /// </summary>
    /// <param name="text">Text to convert</param>
    /// <param name="voiceId">Identifier of the voice to use</param>
    /// <param name="languageCode">Language code (e.g., en-US)</param>
    /// <param name="rate">Speaking rate</param>
    /// <returns>Byte array of the audio content (MP3)</returns>
    Task<byte[]> SynthesizeAsync(string text, string voiceId, string? languageCode = null, double rate = 1.0);
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
