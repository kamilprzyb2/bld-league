using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface ISeasonRepository : IReadWriteRepository<Season>
{
    Task<int?> GetLatestSeasonNumberAsync();
    Task<Season?> GetBySeasonNumberAsync(int seasonNumber);
    Task<IReadOnlyCollection<SeasonSummaryDto>> GetAllSummariesAsync();
    Task<SeasonSummaryDto?> GetSummaryByIdAsync(Guid id);
}
