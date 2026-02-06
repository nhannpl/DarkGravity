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
            var connectionBuilder = new SqlConnectionStringBuilder(connectionString);
            connectionBuilder.Password = dbPassword;
            connectionBuilder.UserID = ConfigConstants.DefaultDbUser; // "sa"
            connectionBuilder.TrustServerCertificate = true;
            connectionBuilder.Encrypt = false;
            connectionString = connectionBuilder.ConnectionString;
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHttpClient();

        services.AddScoped<IStoryAnalyzer, StoryAnalyzer>();
        services.AddScoped<MigrationService>();

        // MassTransit + Kafka
        services.AddMassTransit(x =>
        {
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<StoryFetchedConsumer>();
                rider.AddProducer<StoryFetched>(ConfigConstants.KafkaTopicStoryFetched);

                rider.UsingKafka((context, k) =>
                {
                    k.Host(config[ConfigConstants.KafkaBootstrapServers] ?? "127.0.0.1:9092");
                    k.ClientId = "darkgravity-analyzer";

                    k.TopicEndpoint<StoryFetched>(ConfigConstants.KafkaTopicStoryFetched, "analyzer-group", e =>
                    {
                        // Ensure topic is created with at least 1 partition
                        e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;

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
