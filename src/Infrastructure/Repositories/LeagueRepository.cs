using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Leagues.GetAll;
using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class LeagueRepository(AppDbContext context) :
    ReadWriteRepositoryBase<League>(context), ILeagueRepository
{
    public async Task<IReadOnlyCollection<LeagueSummaryDto>> GetAllSummariesAsync()
        => await DbSet
            .OrderBy(l => l.LeagueIdentifier)
            .Select(l => new LeagueSummaryDto(l.Id, l.LeagueIdentifier, l.LeagueName))
            .ToListAsync();

    public async Task<LeagueSummaryDto?> GetSummaryByIdAsync(Guid id)
        => await DbSet
            .Where(l => l.Id == id)
            .Select(l => new LeagueSummaryDto(l.Id, l.LeagueIdentifier, l.LeagueName))
            .FirstOrDefaultAsync();
}
