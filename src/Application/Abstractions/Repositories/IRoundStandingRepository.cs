using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface IRoundStandingRepository : IReadWriteRepository<RoundStanding>
{
    Task<IReadOnlyCollection<RoundStanding>> GetRoundStandingsByRoundId(Guid roundId);

    Task<IReadOnlyCollection<(Guid, int)>> GetBonusPointsForLeagueSeasonAsync(Guid leagueId, Guid seasonId);
}