using Microsoft.EntityFrameworkCore;
using PortfolioService.Models;

namespace PortfolioService.Data;

public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options) { }

    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<PortfolioItem> PortfolioItems => Set<PortfolioItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Portfolio>()
            .HasMany(p => p.Items)
            .WithOne(i => i.Portfolio)
            .HasForeignKey(i => i.PortfolioId);

        modelBuilder.Entity<Portfolio>()
            .HasIndex(p => p.UserId)
            .IsUnique();
    }
}
