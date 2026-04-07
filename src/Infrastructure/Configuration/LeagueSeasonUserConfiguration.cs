using BldLeague.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BldLeague.Infrastructure.Configuration;

public class LeagueSeasonUserConfiguration : IEntityTypeConfiguration<LeagueSeasonUser>
{
    public void Configure(EntityTypeBuilder<LeagueSeasonUser> b)
    {
        b.ToTable("league_season_users");
        b.HasKey(lsu => lsu.Id);
        b.Property(lsu => lsu.Id).ValueGeneratedNever();

        b.HasOne(lsu => lsu.User)
            .WithMany(u => u.LeagueSeasonUsers)
            .HasForeignKey(lsu => lsu.UserId)
            .IsRequired();
        
        b.HasOne(lsu => lsu.LeagueSeason)
            .WithMany(ls => ls.LeagueSeasonUsers)
            .HasForeignKey(lsu => lsu.LeagueSeasonId)
            .IsRequired();
        
        b.Property(lsu => lsu.SubleagueGroup).HasDefaultValue(0).IsRequired();

        b.HasIndex(lsu => new { lsu.LeagueSeasonId, lsu.UserId })
            .IsUnique();
    }
}