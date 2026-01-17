namespace Shared.Constants;

/// <summary>
/// Centralized configuration keys to ensure consistency and avoid literal string usage.
/// </summary>
public static class ConfigConstants
{
    /// <summary>
    /// The key for the database password environment variable or secret.
    /// </summary>
    public const string DbPasswordKey = "DARKGRAVITY_DB_PASSWORD";

    /// <summary>
    /// The key for the default connection string in appsettings.json.
    /// </summary>
    public const string DefaultConnectionKey = "DefaultConnection";

    /// <summary>
    /// The default database user.
    /// </summary>
    public const string DefaultDbUser = "sa";

    // AI Analysis Constants
    public const string NoAnalysisAvailable = "No analysis available.";
    public const string MockAnalysisPrefix = "MOCK ANALYSIS:";
    public const string ProviderGemini = "Gemini";
    public const string ProviderDeepSeek = "DeepSeek";
    public const string ProviderOpenRouter = "OpenRouter";
    public const string ProviderCloudflare = "Cloudflare";
    public const string ProviderHuggingFace = "HuggingFace";
    public const string ProviderOpenAI = "OpenAI";

    // Error detection keywords
    public static readonly string[] ErrorKeywords = { "Error:", "Exception:", "429", "RESOURCE_EXHAUSTED", "Quota" };
}


