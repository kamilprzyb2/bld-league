using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Rounds.GetDetail;

/// <summary>
/// Data transfer object representing a single player's standing in a round, including all five solve results, best, average, place, and points.
/// </summary>
public class RoundStandingDto
{
    public required string UserFullName { get; set; }
    public required string LeagueIdentifier { get; set; }
    public SolveResult Solve1 { get; set; }
    public SolveResult Solve2 { get; set; }
    public SolveResult Solve3 { get; set; }
    public SolveResult Solve4 { get; set; }
    public SolveResult Solve5 { get; set; }
    public SolveResult Best { get; set; }
    public SolveResult Average { get; set; }
    public int? Place { get; set; }
    public int Points { get; set; }
}
