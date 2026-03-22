using BldLeague.Application.Queries.Matches.GetMatchSummaries;
using BldLeague.Application.Queries.Rounds.GetScrambles;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Matches.GetMatchDetailsById;

/// <summary>
/// Full detail data transfer object for a match, including solve results, averages, scrambles, and season/league context.
/// </summary>
public class MatchDetailsDto : MatchSummaryDto
{
    public Guid SeasonId { get; set; }
    public Guid LeagueId { get; set; }
    public int RoundNumber { get; set; }
    public required string SeasonName { get; set; }
    public required string LeagueName { get; set; }
    public required string RoundName { get; set; }
    public required List<SolveResult> SolvesUserA { get; set; }
    public required List<SolveResult> SolvesUserB { get; set; }
    public required SolveResult UserABest { get; set; }
    public required SolveResult UserBBest { get; set; }
    public required SolveResult UserAAverage { get; set; }
    public required SolveResult UserBAverage { get; set; }
    public List<ScrambleDto> Scrambles { get; set; } = new List<ScrambleDto>();
}
