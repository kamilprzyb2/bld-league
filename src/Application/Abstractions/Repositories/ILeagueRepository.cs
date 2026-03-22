using BldLeague.Application.Queries.Leagues.GetAll;
using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface ILeagueRepository : IReadWriteRepository<League>
{
    Task<IReadOnlyCollection<LeagueSummaryDto>> GetAllSummariesAsync();
    Task<LeagueSummaryDto?> GetSummaryByIdAsync(Guid id);
}
