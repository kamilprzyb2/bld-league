using BldLeague.Application.Queries.Users.GetAll;
using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface ILeagueSeasonUserRepository : IReadWriteRepository<LeagueSeasonUser>
{
    Task<IReadOnlyCollection<UserSummaryDto>> GetUsersByLeagueSeasonIdAsync(Guid leagueSeasonId);
    Task<LeagueSeasonUser?> GetAsync(Guid leagueSeasonId, Guid userId);
}
