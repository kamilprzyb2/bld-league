using BldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> b)
    {
        b.ToTable("seasons");
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).ValueGeneratedNever();

        b.Property(s => s.SeasonNumber).IsRequired();
        b.HasIndex(s => s.SeasonNumber).IsUnique();
    }
}