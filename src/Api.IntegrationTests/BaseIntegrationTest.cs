using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Data;
using Testcontainers.MsSql;
using Xunit;

namespace Api.IntegrationTests;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    protected HttpClient Client { get; private set; } = null!;
    protected IServiceScope Scope { get; private set; } = null!;
    protected AppDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add the test container database
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseSqlServer(_dbContainer.GetConnectionString());
                    });
                });
            });

        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure the database is created and migrated
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        Scope.Dispose();
        await _dbContainer.StopAsync();
    }
}
