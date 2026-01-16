using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Search;

namespace Crawler.Services;

public interface IYoutubeClientWrapper
{
    IAsyncEnumerable<VideoSearchResult> SearchVideosAsync(string query);
    ValueTask<Video> GetVideoAsync(VideoId videoId);
    ValueTask<ClosedCaptionManifest> GetClosedCaptionManifestAsync(VideoId videoId);
    ValueTask<ClosedCaptionTrack> GetClosedCaptionTrackAsync(ClosedCaptionTrackInfo trackInfo);
}

public class YoutubeClientWrapper : IYoutubeClientWrapper
{
    private readonly YoutubeClient _client;

    public YoutubeClientWrapper()
    {
        _client = new YoutubeClient();
    }

    public IAsyncEnumerable<VideoSearchResult> SearchVideosAsync(string query) => _client.Search.GetVideosAsync(query);
    public ValueTask<Video> GetVideoAsync(VideoId videoId) => _client.Videos.GetAsync(videoId);
    public ValueTask<ClosedCaptionManifest> GetClosedCaptionManifestAsync(VideoId videoId) => _client.Videos.ClosedCaptions.GetManifestAsync(videoId);
    public ValueTask<ClosedCaptionTrack> GetClosedCaptionTrackAsync(ClosedCaptionTrackInfo trackInfo) => _client.Videos.ClosedCaptions.GetAsync(trackInfo);
}
