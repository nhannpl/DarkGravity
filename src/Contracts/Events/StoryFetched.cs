namespace DarkGravity.Contracts.Events;

/// <summary>
/// Event published when a new story has been successfully fetched from a source (Reddit, YouTube, etc.) 
/// and saved to the primary data store, but has not yet been analyzed by AI.
/// </summary>
/// <param name="StoryId">The internal database primary key.</param>
/// <param name="Title">The original title of the story.</param>
/// <param name="BodyText">The full text content of the story.</param>
/// <param name="Url">The source URL of the story.</param>
public record StoryFetched(
    Guid StoryId,
    string Title, 
    string BodyText, 
    string Url);
