using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Users.GetAll;
using BldLeague.Application.Queries.Users.GetUserDetailByWcaId;
using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) :
    ReadWriteRepositoryBase<User>(context), IUserRepository
{
    public async Task<IReadOnlyCollection<UserSummaryDto>> GetAllSummariesAsync()
        => await DbSet
            .OrderBy(u => u.FullName)
            .Select(u => new UserSummaryDto(u.Id, u.FullName, u.WcaId, u.IsAdmin, u.AvatarUrl, u.AvatarThumbnailUrl))
            .ToListAsync();

    public async Task<UserSummaryDto?> GetSummaryByIdAsync(Guid id)
        => await DbSet
            .Where(u => u.Id == id)
            .Select(u => new UserSummaryDto(u.Id, u.FullName, u.WcaId, u.IsAdmin, u.AvatarUrl, u.AvatarThumbnailUrl))
            .FirstOrDefaultAsync();

    public async Task<IReadOnlyCollection<UserSummaryDto>> GetUnassignedUsersBySeasonIdAsync(Guid seasonId)
        => await DbSet
            .Where(u => u.LeagueSeasonUsers.All(lsu => lsu.LeagueSeason.SeasonId != seasonId))
            .OrderBy(u => u.FullName)
            .Select(u => new UserSummaryDto(u.Id, u.FullName, u.WcaId, u.IsAdmin, u.AvatarUrl, u.AvatarThumbnailUrl))
            .ToListAsync();

    public async Task<UserDetailDto?> GetUserDetailByWcaIdAsync(string wcaId)
        => await DbSet
            .Where(u => u.WcaId == wcaId)
            .Select(u => new UserDetailDto
            {
                Id = u.Id,
                FullName = u.FullName,
                WcaId = u.WcaId,
                AvatarUrl = u.AvatarUrl,
                AvatarThumbnailUrl = u.AvatarThumbnailUrl,
                IsAdmin = u.IsAdmin
            })
            .FirstOrDefaultAsync();
}
