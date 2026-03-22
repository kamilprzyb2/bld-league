using BldLeague.Application.Queries.Users.GetAll;
using BldLeague.Application.Queries.Users.GetUserDetailByWcaId;
using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface IUserRepository : IReadWriteRepository<User>
{
    Task<IReadOnlyCollection<UserSummaryDto>> GetAllSummariesAsync();
    Task<UserSummaryDto?> GetSummaryByIdAsync(Guid id);
    Task<IReadOnlyCollection<UserSummaryDto>> GetUnassignedUsersBySeasonIdAsync(Guid seasonId);
    Task<UserDetailDto?> GetUserDetailByWcaIdAsync(string wcaId);
}
