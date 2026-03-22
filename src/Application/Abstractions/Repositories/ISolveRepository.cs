using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Abstractions.Repositories;

public interface ISolveRepository : IReadWriteRepository<Solve>
{
    public Task<IReadOnlyCollection<(Guid, SolveResult)>> GetBestSolvesForLeagueSeason(Guid leagueSeasonId);
}