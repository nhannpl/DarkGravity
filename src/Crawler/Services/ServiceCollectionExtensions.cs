using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Shared.Data;
using Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

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
        services.AddScoped<IStoryAnalyzer, StoryAnalyzer>();
        services.AddScoped<IStoryProcessor, StoryProcessor>();
        services.AddScoped<ICrawlerApp, CrawlerApp>();

        return services;
    }
}

