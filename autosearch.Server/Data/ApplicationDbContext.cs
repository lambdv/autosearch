using autosearch.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace autosearch.Data;

public class ApplicationDbContext : DbContext
{
    // public DbSet<Listing> Listings { get; set; }
    public DbSet<CacheEntry> CacheEntries { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<CacheEntry>().HasKey(c => c.Key);
        modelBuilder.Entity<CacheEntry>().Property(c => c.Key).IsRequired();
        modelBuilder.Entity<CacheEntry>().Property(c => c.Value).IsRequired();
        modelBuilder.Entity<CacheEntry>().Property(c => c.CreatedAt).IsRequired();
        modelBuilder.Entity<CacheEntry>().HasIndex(c => c.ExpiresAt);
    }
}


