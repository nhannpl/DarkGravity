using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Shared.Data;
using Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Analyzer.Services;
using MassTransit;
using DarkGravity.Contracts.Events;
using Analyzer.Consumers;

namespace Analyzer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnalyzerServices(this IServiceCollection services, IConfiguration config)
    {
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

        services.AddHttpClient();

        services.AddScoped<IStoryAnalyzer, StoryAnalyzer>();

        // MassTransit + Kafka
        services.AddMassTransit(x =>
        {
            x.AddConsumer<StoryFetchedConsumer>();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<StoryFetchedConsumer>();

                rider.UsingKafka((context, k) =>
                {
                    k.Host(config[ConfigConstants.KafkaBootstrapServers] ?? "localhost:9092");

                    k.TopicConsumer<StoryFetched>(ConfigConstants.KafkaTopicStoryFetched, "analyzer-group", e =>
                    {
                        e.ConfigureConsumer<StoryFetchedConsumer>(context);

                        // Resiliency (Polly-like behavior built into MassTransit)
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    });
                });
            });
        });

        return services;
    }
}
