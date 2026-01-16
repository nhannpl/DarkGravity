
using Shared.Models;
using System.Text.Json;

namespace Crawler.Services;

public interface IRedditService
{
    Task<List<Story>> GetTopStoriesAsync(string url);
}

public class RedditService : IRedditService
{
    private readonly HttpClient _http;

    public RedditService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Story>> GetTopStoriesAsync(string url)
    {
        var stories = new List<Story>();
        try
        {
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            var posts = doc.RootElement.GetProperty("data").GetProperty("children");

            foreach (var post in posts.EnumerateArray())
            {
                var data = post.GetProperty("data");
                if (data.GetProperty("stickied").GetBoolean()) continue;

                var story = new Story
                {
                    ExternalId = data.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(),
                    Title = data.GetProperty("title").GetString() ?? "Unknown",
                    Author = data.GetProperty("author").GetString() ?? "Unknown",
                    Url = "https://reddit.com" + data.GetProperty("permalink").GetString(),
                    BodyText = data.GetProperty("selftext").GetString() ?? "",
                    Upvotes = data.GetProperty("ups").GetInt32()
                };

                stories.Add(story);
            }
            return stories;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching stories: {ex.Message}");
            return new List<Story>();
        }
    }
}
