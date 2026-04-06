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
        var groups = await DbSet
            .Where(s => s.Match.LeagueSeasonId == leagueSeasonId)
            .GroupBy(s => s.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                BestValid = g.Where(s => s.Result > 0).Min(s => (int?)s.Result),
                HasDnf = g.Any(s => s.Result == -1)
            })
            .ToListAsync();

        return groups
            .Select(g =>
            {
                SolveResult best = g.BestValid.HasValue
                    ? SolveResult.FromCentiseconds(g.BestValid.Value)
                    : g.HasDnf ? SolveResult.Dnf() : SolveResult.Dns();
                return (g.UserId, best);
            })
            .ToList();
    }
}