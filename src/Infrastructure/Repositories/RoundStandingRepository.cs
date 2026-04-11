using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Commands.PlayerRankings.Refresh;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
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

    public async Task<IReadOnlyCollection<BestSinglePerUserDto>> GetBestSinglePerUserAsync()
    {
        return await DbSet
            .Where(rs => rs.Best >= (SolveResult)0)
            .GroupBy(rs => rs.UserId)
            .Select(g => g.OrderBy(rs => rs.Best).First())
            .Select(rs => new BestSinglePerUserDto(rs.UserId, rs.Best, rs.RoundId))
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<BestAveragePerUserDto>> GetBestAveragePerUserAsync()
    {
        return await DbSet
            .Where(rs => rs.Average >= (SolveResult)0)
            .GroupBy(rs => rs.UserId)
            .Select(g => g.OrderBy(rs => rs.Average).First())
            .Select(rs => new BestAveragePerUserDto(
                rs.UserId, rs.Average, rs.RoundId,
                rs.Solve1, rs.Solve2, rs.Solve3, rs.Solve4, rs.Solve5))
            .ToListAsync();
    }
}