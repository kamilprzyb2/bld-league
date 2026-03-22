using BldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> b)
    {
        b.ToTable("rounds");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedNever();

        b.HasOne(r => r.Season)
            .WithMany(s=>s.Rounds)
            .HasForeignKey(r => r.SeasonId)
            .IsRequired();

        b.Property(r => r.RoundNumber).IsRequired();
        
        b.Property(r => r.StartDate).IsRequired();
        
        b.Property(r => r.EndDate).IsRequired();
        
        b.HasIndex(r => new { r.SeasonId, r.RoundNumber })
            .IsUnique();
    }
}