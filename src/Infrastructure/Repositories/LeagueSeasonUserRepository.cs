using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Users.GetByLeagueSeasonId;
using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class LeagueSeasonUserRepository(AppDbContext context) :
    ReadWriteRepositoryBase<LeagueSeasonUser>(context), ILeagueSeasonUserRepository
{
    public async Task<IReadOnlyCollection<LeagueSeasonUserDto>> GetUsersByLeagueSeasonIdAsync(Guid leagueSeasonId)
        => await DbSet
            .Where(lsu => lsu.LeagueSeasonId == leagueSeasonId)
            .OrderBy(lsu => lsu.User.FullName)
            .Select(lsu => new LeagueSeasonUserDto(
                lsu.User.Id,
                lsu.User.FullName,
                lsu.User.WcaId,
                lsu.SubleagueGroup))
            .ToListAsync();

    public async Task<LeagueSeasonUser?> GetAsync(Guid leagueSeasonId, Guid userId)
        => await DbSet
            .Where(lsu => lsu.LeagueSeasonId == leagueSeasonId && lsu.UserId == userId)
            .FirstOrDefaultAsync();
}
