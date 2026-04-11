using BldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Context;

/// <inheritdoc />
public class AppDbContext : DbContext
{
    /// <inheritdoc />
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PlayerRanking> PlayerRankings => Set<PlayerRanking>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IAssemblyMarker).Assembly);
    }
}