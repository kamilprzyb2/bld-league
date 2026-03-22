using BldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class LeagueSeasonConfiguration : IEntityTypeConfiguration<LeagueSeason>
{
    public void Configure(EntityTypeBuilder<LeagueSeason> b)
    {
        b.ToTable("league_seasons");
        b.HasKey(ls => ls.Id);
        b.Property(ls => ls.Id).ValueGeneratedNever();

        b.HasOne(ls => ls.League)
            .WithMany(l => l.LeagueSeasons)
            .HasForeignKey(ls => ls.LeagueId)
            .IsRequired();
        
        b.HasOne(ls => ls.Season)
            .WithMany(s => s.LeagueSeasons)
            .HasForeignKey(ls => ls.SeasonId)
            .IsRequired();
        
        // Enforce unique League+Season combination
        b.HasIndex(ls => new { ls.LeagueId, ls.SeasonId })
            .IsUnique();
    }
}