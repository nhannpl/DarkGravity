using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Api.Extensions;
using Shared.Data;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Api.IntegrationTests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddApiServices_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        var configData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;",
            ["DARKGRAVITY_DB_PASSWORD"] = "SecretPass123!",
            ["AllowedOrigins:0"] = "https://example.com"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        services.AddApiServices(config, isDevelopment: false);
        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(AppDbContext));
        Assert.Contains(services, d => d.ServiceType == typeof(Microsoft.AspNetCore.Cors.Infrastructure.ICorsService));
        Assert.Contains(services, d => d.ServiceType == typeof(Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider));
    }
}
