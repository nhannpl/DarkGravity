using Microsoft.EntityFrameworkCore;

namespace Crawler;

public class Story
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ExternalId { get; set; } = string.Empty; // Reddit ID (e.g., "t3_xyz")
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public int Upvotes { get; set; }
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}

public class AppDbContext : DbContext
{
    public DbSet<Story> Stories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connecting to the Docker container we just started
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=DarkGravityDb;User Id=sa;Password=REMOVED_PASSWORD;TrustServerCertificate=True;");
    }
}
