using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface ILeagueSeasonStandingRepository : IReadWriteRepository<LeagueSeasonStanding>
{
    Task<IReadOnlyCollection<LeagueSeasonStanding>> GetStandingsByLeagueSeasonIdAsync(Guid leagueSeasonId);
}