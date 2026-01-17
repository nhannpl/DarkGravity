using Microsoft.Extensions.DependencyInjection;
using Shared.Data;
using Shared.Constants;
using Microsoft.EntityFrameworkCore;

using Microsoft.Data.SqlClient;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config, bool isDevelopment)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Database
        var connectionString = config.GetConnectionString(ConfigConstants.DefaultConnectionKey);
        var dbPassword = config[ConfigConstants.DbPasswordKey];

        if (!string.IsNullOrEmpty(dbPassword))
        {
            var connectionBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                Password = dbPassword
            };
            connectionString = connectionBuilder.ConnectionString;
        }


        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                if (isDevelopment)
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
                else
                {
                    var allowedOrigins = config.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                    if (allowedOrigins.Any())
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                }
            });
        });

        return services;
    }
}
