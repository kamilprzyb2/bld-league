using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> b)
    {
        b.ToTable("matches");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedNever();

        b.HasOne(m => m.LeagueSeason)
            .WithMany(ls => ls.Matches)
            .HasForeignKey(m => m.LeagueSeasonId)
            .IsRequired();

        b.HasOne(m => m.Round)
            .WithMany(r => r.Matches)
            .HasForeignKey(m => m.RoundId)
            .IsRequired();

        b.HasOne(m => m.UserA)
            .WithMany(u => u.MatchesAsUserA)
            .HasForeignKey(m => m.UserAId)
            .IsRequired();

        b.HasOne(m => m.UserB)
            .WithMany(u => u.MatchesAsUserB)
            .HasForeignKey(m => m.UserBId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(m => m.UserAScore)
            .IsRequired();
        
        b.Property(m => m.UserABest)
            .HasConversion(new SolveResultConverter())
            .IsRequired();

        b.Property(m => m.UserBBest)
            .HasConversion(new SolveResultConverter())
            .IsRequired();
        
        b.Property(m=>m.UserAAverage)
            .HasConversion(new SolveResultConverter())
            .IsRequired();

        b.Property(m => m.UserBAverage)
            .HasConversion(new SolveResultConverter())
            .IsRequired();
    }
}