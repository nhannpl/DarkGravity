using Shared.Constants;

namespace Api.Models;

public class StoryQueryParameters
{
    public string? SearchTerm { get; set; }
    public double? MinScaryScore { get; set; }
    public string? SortBy { get; set; } = StorySortFields.Upvotes;
    public string? SortOrder { get; set; } = SortOrders.Descending;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
