using Microsoft.EntityFrameworkCore;
using Shared.Models;
using MassTransit;

namespace Shared.Data;

public class AppDbContext : DbContext
{
    public AppDbContext() { }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Story> Stories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MassTransit Outbox tables
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    // No longer overriding OnConfiguring with fallback strings to ensure security and proper DI usage.

}
