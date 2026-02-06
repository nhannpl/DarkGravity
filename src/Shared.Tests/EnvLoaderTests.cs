using Shared.Helpers;
using Xunit;

namespace Shared.Tests;

public class EnvLoaderTests : IDisposable
{
    private readonly string _testEnvFile = "test.env";

    public EnvLoaderTests()
    {
        if (File.Exists(_testEnvFile)) File.Delete(_testEnvFile);
    }

    public void Dispose()
    {
        if (File.Exists(_testEnvFile)) File.Delete(_testEnvFile);
    }

    [Fact]
    public void Load_SetsEnvironmentVariables_FromValidFile()
    {
        // Arrange
        var key = "TEST_KEY_" + Guid.NewGuid();
        var value = "TEST_VALUE";
        File.WriteAllText(_testEnvFile, $"{key}={value}");

        // Act
        EnvLoader.Load(_testEnvFile);

        // Assert
        Assert.Equal(value, Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void Load_SkipsComments()
    {
        // Arrange
        var key = "TEST_KEY_COMMENT_" + Guid.NewGuid();
        File.WriteAllText(_testEnvFile, $"# {key}=VALUE\n{key}=REAL_VALUE");

        // Act
        EnvLoader.Load(_testEnvFile);

        // Assert
        Assert.Equal("REAL_VALUE", Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void Load_DoesNotOverwriteExistingVariables()
    {
        // Arrange
        var key = "TEST_KEY_EXISTS_" + Guid.NewGuid();
        Environment.SetEnvironmentVariable(key, "ORIGINAL");
        File.WriteAllText(_testEnvFile, $"{key}=NEW");

        // Act
        EnvLoader.Load(_testEnvFile);

        // Assert
        Assert.Equal("ORIGINAL", Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void Load_GracefullyHandlesMissingFile()
    {
        // Act & Assert (Should not throw)
        var exception = Record.Exception(() => EnvLoader.Load("non-existent.env"));
        Assert.Null(exception);
    }
}
