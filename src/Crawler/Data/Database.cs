using Microsoft.EntityFrameworkCore;
using Crawler.Models;

namespace Crawler.Data;

public class AppDbContext : DbContext
{
    public DbSet<Story> Stories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connecting to the Docker container we just started
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=DarkGravityDb;User Id=sa;Password=REMOVED_PASSWORD;TrustServerCertificate=True;");
    }
}
