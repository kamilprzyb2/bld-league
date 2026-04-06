using BldLeague.Application.Queries.Matches.GetAll;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Application.Queries.Matches.GetMatchExport;
using BldLeague.Application.Queries.Matches.GetMatchSummaries;
using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface IMatchRepository : IReadWriteRepository<Match>
{
    Task<IReadOnlyCollection<MatchAdminSummaryDto>> GetAllAdminSummariesAsync();
    Task<IReadOnlyCollection<MatchExportRowDto>> GetMatchExportRowsAsync(int? seasonNumber = null, string? leagueIdentifier = null, int? roundNumber = null);
    Task<MatchDetailsDto?> GetMatchDetailsByIdAsync(Guid id);
    Task<IReadOnlyCollection<MatchSummaryDto>> GetMatchSummaries(Guid? seasonId, Guid? leagueId, int? roundNumber);
    Task<IReadOnlyCollection<Match>> GetMatchesByRoundIdAsync(Guid roundId);
    Task<IReadOnlyCollection<Match>> GetMatchesByLeagueSeasonAsync(Guid leagueSeasonId);
    Task<IReadOnlyCollection<Match>> GetFinishedMatchesByLeagueSeasonAsync(Guid leagueSeasonId, DateTime cutoff);
    Task<Match?> GetMatchWithSolvesAsync(Guid id);
}
