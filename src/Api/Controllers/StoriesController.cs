using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Models;
using Shared.Constants;
using Api.Models;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public StoriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/stories
    [HttpGet]
    public async Task<ActionResult<PagedResult<Story>>> GetStories([FromQuery] StoryQueryParameters @params)
    {
        var query = _context.Stories.AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(@params.SearchTerm))
        {
            query = query.Where(s => s.Title.Contains(@params.SearchTerm) || s.BodyText.Contains(@params.SearchTerm));
        }

        if (@params.MinScaryScore.HasValue)
        {
            query = query.Where(s => s.ScaryScore >= @params.MinScaryScore.Value);
        }

        // Sorting
        query = @params.SortBy switch
        {
            StorySortFields.ScaryScore => @params.SortOrder == SortOrders.Ascending
                ? query.OrderBy(s => s.ScaryScore)
                : query.OrderByDescending(s => s.ScaryScore),

            StorySortFields.FetchedAt => @params.SortOrder == SortOrders.Ascending
                ? query.OrderBy(s => s.FetchedAt)
                : query.OrderByDescending(s => s.FetchedAt),

            StorySortFields.Title => @params.SortOrder == SortOrders.Ascending
                ? query.OrderBy(s => s.Title)
                : query.OrderByDescending(s => s.Title),

            _ => @params.SortOrder == SortOrders.Ascending
                ? query.OrderBy(s => s.Upvotes)
                : query.OrderByDescending(s => s.Upvotes)
        };

        // Pagination
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((@params.Page - 1) * @params.PageSize)
            .Take(@params.PageSize)
            .ToListAsync();

        return new PagedResult<Story>
        {
            Items = items,
            TotalCount = totalCount,
            Page = @params.Page,
            PageSize = @params.PageSize
        };
    }

    // GET: api/stories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Story>> GetStory(Guid id)
    {
        var story = await _context.Stories.FindAsync(id);

        if (story == null)
        {
            return NotFound();
        }

        return story;
    }
}
