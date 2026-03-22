using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class RoundStandingConfiguration : IEntityTypeConfiguration<RoundStanding>
{
    public void Configure(EntityTypeBuilder<RoundStanding> b)
    {
        b.ToTable("round_standings");
        b.HasKey(rs => rs.Id);
        b.Property(rs => rs.Id).ValueGeneratedNever();
        
        b.HasOne(rs=>rs.Round)
            .WithMany(r => r.Standings)
            .HasForeignKey(rs=>rs.RoundId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(rs=>rs.User)
            .WithMany(u => u.RoundStandings)
            .HasForeignKey(rs=>rs.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(rs=>rs.League)
            .WithMany(l=>l.RoundStandings)
            .HasForeignKey(rs=>rs.LeagueId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        b.Property(rs => rs.Points)
            .IsRequired();
        
        b.Property(rs => rs.Place)
            .IsRequired();
        
        b.Property(rs => rs.Solve1)
            .HasConversion(new SolveResultConverter())
            .IsRequired();
        
        b.Property(rs => rs.Solve2)
            .HasConversion(new SolveResultConverter())
            .IsRequired();
        
        b.Property(rs => rs.Solve3)
            .HasConversion(new SolveResultConverter())
            .IsRequired();
        
        b.Property(rs => rs.Solve4)
            .HasConversion(new SolveResultConverter())
            .IsRequired();
        
        b.Property(rs => rs.Solve5)
            .HasConversion(new SolveResultConverter())
            .IsRequired();
        
        b.Property(rs => rs.Best)
            .HasConversion(new SolveResultConverter())
            .IsRequired();

        b.Property(rs => rs.Average)
            .HasConversion(new SolveResultConverter())
            .IsRequired();
    }
}