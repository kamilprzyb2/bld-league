using BldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u=>u.Id);
        b.Property(u => u.Id).ValueGeneratedNever();

        b.Property(u => u.FullName).HasMaxLength(255).IsRequired();

        b.Property(u => u.WcaId).HasMaxLength(10).IsRequired();
        b.HasIndex(u => u.WcaId).IsUnique();
    }
}