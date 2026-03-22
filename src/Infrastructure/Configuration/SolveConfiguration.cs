using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class SolveConfiguration : IEntityTypeConfiguration<Solve>
{
    public void Configure(EntityTypeBuilder<Solve> b)
    {
        b.ToTable("solves");
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).ValueGeneratedNever();

        b.Property(s => s.Result)
            .HasConversion(new SolveResultConverter())
            .IsRequired();

        b.Property(s => s.Index)
            .IsRequired();
        
        b.HasOne(s => s.Match)
            .WithMany(m => m.Solves)
            .HasForeignKey(s => s.MatchId)
            .IsRequired();
        
        b.HasOne(s => s.User)
            .WithMany(u => u.Solves)
            .HasForeignKey(s => s.UserId)
            .IsRequired();
    }
}