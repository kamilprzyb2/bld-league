using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Abstractions.Repositories;

public interface ISolveRepository : IReadWriteRepository<Solve>
{
    public Task<IReadOnlyCollection<(Guid, SolveResult)>> GetBestSolvesForLeagueSeason(Guid leagueSeasonId, DateTime cutoff);
    Task<IReadOnlyCollection<SolveResult>> GetFinishedSolvesByUserIdAsync(Guid userId, DateTime localToday);
    Task<IReadOnlyCollection<Solve>> GetByMatchIdAsync(Guid matchId);
}