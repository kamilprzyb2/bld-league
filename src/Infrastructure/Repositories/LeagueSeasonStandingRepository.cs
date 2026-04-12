using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class LeagueSeasonStandingRepository(AppDbContext context) : 
    ReadWriteRepositoryBase<LeagueSeasonStanding>(context), ILeagueSeasonStandingRepository
{
    public async Task<IReadOnlyCollection<LeagueSeasonStanding>> GetStandingsByLeagueSeasonIdAsync(Guid leagueSeasonId)
    {
        return await DbSet
            .Where(lss => lss.LeagueSeasonId == leagueSeasonId)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<LeagueSeasonStanding>> GetByUserIdWithDetailsAsync(Guid userId)
        => await DbSet
            .Include(lss => lss.LeagueSeason).ThenInclude(ls => ls.League)
            .Include(lss => lss.LeagueSeason).ThenInclude(ls => ls.Season)
            .Where(lss => lss.UserId == userId)
            .OrderByDescending(lss => lss.LeagueSeason.Season.SeasonNumber)
            .ToListAsync();
}