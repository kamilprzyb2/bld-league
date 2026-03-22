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
}