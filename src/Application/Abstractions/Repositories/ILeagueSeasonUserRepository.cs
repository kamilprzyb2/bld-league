using BldLeague.Application.Queries.Users.GetByLeagueSeasonId;
using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface ILeagueSeasonUserRepository : IReadWriteRepository<LeagueSeasonUser>
{
    Task<IReadOnlyCollection<LeagueSeasonUserDto>> GetUsersByLeagueSeasonIdAsync(Guid leagueSeasonId);
    Task<LeagueSeasonUser?> GetAsync(Guid leagueSeasonId, Guid userId);
}
