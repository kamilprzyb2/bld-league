using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class PlayerRankingConfiguration : IEntityTypeConfiguration<PlayerRanking>
{
    public void Configure(EntityTypeBuilder<PlayerRanking> b)
    {
        b.ToTable("PlayerRankings");
        b.HasKey(pr => pr.Id);
        b.Property(pr => pr.Id).ValueGeneratedNever();

        b.HasIndex(pr => pr.UserId).IsUnique();

        b.HasOne(pr => pr.User)
            .WithOne(u => u.PlayerRanking)
            .HasForeignKey<PlayerRanking>(pr => pr.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(pr => pr.SingleRound)
            .WithMany()
            .HasForeignKey(pr => pr.SingleRoundId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(pr => pr.AverageRound)
            .WithMany()
            .HasForeignKey(pr => pr.AverageRoundId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        b.Property(pr => pr.BestSingle)
            .HasConversion(
                v => v != null ? (int?)v.Value.Centiseconds : null,
                v => v.HasValue ? SolveResult.FromCentiseconds(v.Value) : (SolveResult?)null)
            .IsRequired(false);

        b.Property(pr => pr.BestAverage)
            .HasConversion(
                v => v != null ? (int?)v.Value.Centiseconds : null,
                v => v.HasValue ? SolveResult.FromCentiseconds(v.Value) : (SolveResult?)null)
            .IsRequired(false);

        b.Property(pr => pr.AverageSolve1)
            .HasConversion(
                v => v != null ? (int?)v.Value.Centiseconds : null,
                v => v.HasValue ? SolveResult.FromCentiseconds(v.Value) : (SolveResult?)null)
            .IsRequired(false);

        b.Property(pr => pr.AverageSolve2)
            .HasConversion(
                v => v != null ? (int?)v.Value.Centiseconds : null,
                v => v.HasValue ? SolveResult.FromCentiseconds(v.Value) : (SolveResult?)null)
            .IsRequired(false);

        b.Property(pr => pr.AverageSolve3)
            .HasConversion(
                v => v != null ? (int?)v.Value.Centiseconds : null,
                v => v.HasValue ? SolveResult.FromCentiseconds(v.Value) : (SolveResult?)null)
            .IsRequired(false);

        b.Property(pr => pr.AverageSolve4)
            .HasConversion(
                v => v != null ? (int?)v.Value.Centiseconds : null,
                v => v.HasValue ? SolveResult.FromCentiseconds(v.Value) : (SolveResult?)null)
            .IsRequired(false);

        b.Property(pr => pr.AverageSolve5)
            .HasConversion(
                v => v != null ? (int?)v.Value.Centiseconds : null,
                v => v.HasValue ? SolveResult.FromCentiseconds(v.Value) : (SolveResult?)null)
            .IsRequired(false);
    }
}
