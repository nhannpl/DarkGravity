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
}
