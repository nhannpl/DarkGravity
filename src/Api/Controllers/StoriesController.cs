using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Models;

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
    public async Task<ActionResult<IEnumerable<Story>>> GetStories()
    {
        return await _context.Stories
            .OrderByDescending(s => s.Upvotes)
            .Take(50) // Limit to top 50 for now
            .ToListAsync();
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
