using BldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> b)
    {
        b.ToTable("leagues");
        b.HasKey(l => l.Id);
        b.Property(l => l.Id).ValueGeneratedNever();
        
        b.Property(l => l.LeagueIdentifier).HasMaxLength(10).IsRequired();
        b.HasIndex(l => l.LeagueIdentifier).IsUnique();
        b.Ignore(l => l.LeagueName);
    }
}