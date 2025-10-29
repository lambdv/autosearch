using autosearch.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace autosearch.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Listing> Listings { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Listing>().HasKey(l => l.Id);
        modelBuilder.Entity<Listing>().Property(l => l.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Listing>().Property(l => l.Url).IsRequired();
        modelBuilder.Entity<Listing>().Property(l => l.Title).IsRequired();
        modelBuilder.Entity<Listing>().Property(l => l.Price).IsRequired();
    }
}


