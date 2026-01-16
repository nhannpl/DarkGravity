using System.ComponentModel.DataAnnotations;
using Shared.Constants;

namespace Api.Models;

public class StoryQueryParameters
{
    private const int MaxPageSize = 100;
    private const int MaxSearchTermLength = 200;

    [MaxLength(MaxSearchTermLength)]
    public string? SearchTerm { get; set; }

    [Range(0, 10)]
    public double? MinScaryScore { get; set; }

    public string? SortBy { get; set; } = StorySortFields.Upvotes;

    public string? SortOrder { get; set; } = SortOrders.Descending;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize { get; set; } = 50;
}
