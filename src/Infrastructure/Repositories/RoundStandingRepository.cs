using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class RoundStandingRepository(AppDbContext context)
    : ReadWriteRepositoryBase<RoundStanding>(context), IRoundStandingRepository
{
    public async Task<IReadOnlyCollection<RoundStanding>> GetRoundStandingsByRoundId(Guid roundId)
    {
        return await DbSet
            .Where(rs => rs.RoundId == roundId)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<(Guid, int)>> GetBonusPointsForLeagueSeasonAsync(Guid leagueId, Guid seasonId)
    {
        return await DbSet
            .Include(rs => rs.Round)
            .Where(rs => rs.LeagueId == leagueId && rs.Round.SeasonId == seasonId)
            .GroupBy(rs => rs.UserId)
            .Select(g => new ValueTuple<Guid, int>(
                g.Key,
                g.Sum(rs=>rs.Points)))
            .ToListAsync();
    }

}