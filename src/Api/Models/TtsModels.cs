namespace Api.Models;

/// <summary>
/// Request model for text-to-speech synthesis
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
