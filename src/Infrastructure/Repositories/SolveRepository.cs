using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class SolveRepository(AppDbContext context) : 
    ReadWriteRepositoryBase<Solve>(context), ISolveRepository
{
    public async Task<IReadOnlyCollection<(Guid, SolveResult)>> GetBestSolvesForLeagueSeason(Guid leagueSeasonId)
    {
        return await DbSet
            .Where(s => s.Match.LeagueSeasonId == leagueSeasonId && s.Result > 0)
            .GroupBy(s => s.UserId)
            .Select(g => new ValueTuple<Guid, SolveResult>(
                g.Key,
                g.Min(s => s.Result)
            ))
            .ToListAsync();
    }
}