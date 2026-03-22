using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Matches.GetMatchExport;

/// <summary>
/// Data transfer object representing a single match row for CSV export, carrying player WCA IDs and all solve results.
/// </summary>
public class MatchExportRowDto
{
    public int SeasonNumber { get; set; }
    public required string LeagueIdentifier { get; set; }
    public int RoundNumber { get; set; }
    public required string UserAWcaId { get; set; }
    public string? UserBWcaId { get; set; }
    public required List<SolveResult> SolvesUserA { get; set; }
    public required List<SolveResult> SolvesUserB { get; set; }
}
