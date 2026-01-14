using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Shared.Data;

public class AppDbContext : DbContext
{
    public AppDbContext() { }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Story> Stories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback for Console App if not configured via DI
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=DarkGravityDb;User Id=sa;Password=REMOVED_PASSWORD;TrustServerCertificate=True;");
        }
    }
}
