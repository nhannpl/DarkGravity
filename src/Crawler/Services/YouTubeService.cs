using Shared.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;
using System.Text;

namespace Crawler.Services;

public interface IYouTubeService
{
    Task<List<Story>> GetStoriesFromChannelAsync(string query, int limit = 3);
}

public class YouTubeService : IYouTubeService
{
    private readonly YoutubeClient _youtube;

    public YouTubeService()
    {
        _youtube = new YoutubeClient();
    }

    public async Task<List<Story>> GetStoriesFromChannelAsync(string query, int limit = 3)
    {
        var stories = new List<Story>();
        try
        {
            var searchResults = _youtube.Search.GetVideosAsync(query);
            
            int count = 0;
            await foreach (var searchResult in searchResults)
            {
                if (count >= limit) break;

                Console.WriteLine($"   [YT] Fetching details for: {searchResult.Title}");
                
                // Get full video details
                var video = await _youtube.Videos.GetAsync(searchResult.Id);
                var bodyText = video.Description;
                
                // Try to fetch transcripts
                try 
                {
                    var manifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(video.Id);
                    var trackInfo = manifest.TryGetByLanguage("en");
                    if (trackInfo != null)
                    {
                        var track = await _youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
                        var sb = new StringBuilder();
                        foreach (var caption in track.Captions)
                        {
                            var text = caption.Text.Replace("\n", " ").Trim();
                            if (string.IsNullOrWhiteSpace(text)) continue;
                            
                            sb.Append(text).Append(" ");
                        }
                        bodyText = sb.ToString();
                    }
                }
                catch 
                {
                    // Fallback to description already set
                }

                stories.Add(new Story
                {
                    ExternalId = "yt_" + video.Id,
                    Title = video.Title,
                    Author = video.Author.ChannelTitle,
                    Url = video.Url,
                    BodyText = string.IsNullOrWhiteSpace(bodyText) ? "No content available." : bodyText,
                    Upvotes = (int?)video.Engagement.ViewCount ?? 0,
                    FetchedAt = DateTime.UtcNow
                });
                
                count++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching YouTube stories: {ex.Message}");
        }

        return stories;
    }
}
