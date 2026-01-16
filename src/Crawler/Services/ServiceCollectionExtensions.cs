using Microsoft.Extensions.DependencyInjection;
using Shared.Data;

namespace Crawler.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrawlerServices(this IServiceCollection services)
    {
        services.AddScoped<AppDbContext>();
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
