using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Shared.Data;

public class AppDbContext : DbContext
{
    public AppDbContext() { }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Story> Stories { get; set; }

    // No longer overriding OnConfiguring with fallback strings to ensure security and proper DI usage.

}
