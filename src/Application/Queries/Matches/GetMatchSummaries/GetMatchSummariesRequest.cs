using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Matches.GetMatchSummaries;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetMatchSummaries;

/// <summary>
/// Request to retrieve match summaries for the public view, optionally filtered by season, league, and round number.
/// </summary>
public class GetMatchSummariesRequest(Guid? seasonId = null, Guid? leagueId = null, int? roundNumber = null)
    : IRequest<IReadOnlyCollection<MatchSummaryDto>>
{
    public Guid? SeasonId { get; } = seasonId;
    public Guid? LeagueId { get; } = leagueId;
    public int? RoundNumber { get; } = roundNumber;
}
