using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Shared.Data;
using Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using MassTransit;
using DarkGravity.Contracts.Events;

namespace Crawler.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrawlerServices(this IServiceCollection services, IConfiguration config)
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

        services.AddSingleton<IYoutubeClientWrapper, YoutubeClientWrapper>();
        services.AddHttpClient<IRedditService, RedditService>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "DarkGravityCrawler/1.0");
        });
        services.AddScoped<IYouTubeService, YouTubeService>();
        services.AddScoped<IStoryProcessor, StoryProcessor>();
        services.AddScoped<ICrawlerApp, CrawlerApp>();

        // MassTransit + Kafka
        services.AddMassTransit(x =>
        {
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddProducer<StoryFetched>(ConfigConstants.KafkaTopicStoryFetched);

                rider.UsingKafka((context, k) =>
                {
                    k.Host(config[ConfigConstants.KafkaBootstrapServers] ?? "localhost:9092");
                });
            });

            // Outbox Pattern
            x.AddEntityFrameworkOutbox<AppDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });
        });

        return services;
    }
}

