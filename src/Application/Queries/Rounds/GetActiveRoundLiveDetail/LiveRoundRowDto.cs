using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Rounds.GetActiveRoundLiveDetail;

/// <summary>
/// Represents a single user-row in the active (not-yet-finished) round live view. For users in the
/// "finished" section all solve/best/average fields are populated; for users in the
/// "submitted alone" or "pending" sections only the identity fields are meaningful.
/// </summary>
public class LiveRoundRowDto
{
    public Guid UserId { get; set; }
    public required string UserFullName { get; set; }
    public required string LeagueIdentifier { get; set; }
    public SolveResult Solve1 { get; set; }
    public SolveResult Solve2 { get; set; }
    public SolveResult Solve3 { get; set; }
    public SolveResult Solve4 { get; set; }
    public SolveResult Solve5 { get; set; }
    public SolveResult Best { get; set; }
    public SolveResult Average { get; set; }
}
