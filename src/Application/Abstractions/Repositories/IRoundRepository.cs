using BldLeague.Application.Queries.Rounds.GetAll;
using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using BldLeague.Application.Queries.Rounds.GetDetail;
using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface IRoundRepository : IReadWriteRepository<Round>
{
    Task<IReadOnlyCollection<RoundSummaryDto>> GetRoundSummariesBySeasonIdAsync(Guid seasonId);
    Task<IReadOnlyCollection<RoundAdminSummaryDto>> GetAllRoundSummariesAsync();
    Task<RoundSummaryDto?> GetSummaryByIdAsync(Guid id);
    Task<int?> GetLatestRoundNumberAsync();
    Task<RoundDetailDto?> GetRoundDetailAsync(Guid seasonId, int roundNumber);
}
