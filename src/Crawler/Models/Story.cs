namespace Crawler.Models;

public class Story
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ExternalId { get; set; } = string.Empty; // Reddit ID (e.g., "t3_xyz")
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public string AiAnalysis { get; set; } = string.Empty; // New AI Field
    public int Upvotes { get; set; }
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
