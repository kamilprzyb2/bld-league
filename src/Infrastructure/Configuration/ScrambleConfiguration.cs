using BldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class ScrambleConfiguration : IEntityTypeConfiguration<Scramble>
{
    public void Configure(EntityTypeBuilder<Scramble> b)
    {
        b.ToTable("scrambles");
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).ValueGeneratedNever();

        b.HasOne(s => s.Round)
            .WithMany(r => r.Scrambles)
            .HasForeignKey(s => s.RoundId)
            .IsRequired();

        b.Property(s => s.ScrambleNumber).IsRequired();
        b.Property(s => s.Notation).IsRequired();

        b.HasIndex(s => new { s.RoundId, s.ScrambleNumber }).IsUnique();
    }
}
