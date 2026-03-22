using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class LeagueSeasonStandingConfiguration : IEntityTypeConfiguration<LeagueSeasonStanding>
{
    public void Configure(EntityTypeBuilder<LeagueSeasonStanding> b)
    {
        b.ToTable("league_season_standings");
        b.HasKey(lss => lss.Id);
        b.Property(lss => lss.Id)
            .ValueGeneratedNever();

        b.HasOne(lss => lss.LeagueSeason)
            .WithMany(ls => ls.LeagueSeasonStandings)
            .HasForeignKey(lss => lss.LeagueSeasonId)
            .IsRequired();
        
        b.HasOne(lss => lss.User)
            .WithMany(u => u.LeagueSeasonStandings)
            .HasForeignKey(lss => lss.UserId)
            .IsRequired();

        b.Property(lss => lss.Place)
            .IsRequired();
        
        b.Property(lss => lss.MatchesPlayed)
            .IsRequired();
        
        b.Property(lss => lss.MatchesWon)
            .IsRequired();
        
        b.Property(lss => lss.MatchesTied)
            .IsRequired();
        
        b.Property(lss => lss.MatchesLost)
            .IsRequired();
        
        b.Property(lss => lss.BigPoints)
            .IsRequired();
        
        b.Property(lss => lss.BonusPoints)
            .IsRequired();
        
        b.Property(lss => lss.SmallPointsGained)
            .IsRequired();
        
        b.Property(lss => lss.SmallPointsLost)
            .IsRequired();
        
        b.Property(lss => lss.SmallPointsBalance)
            .IsRequired();
        
        b.Property(lss => lss.Best)
            .HasConversion(new SolveResultConverter())
            .IsRequired();
    }
}