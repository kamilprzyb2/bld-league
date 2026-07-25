using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Users.GetHeadToHeadComparison;

/// <summary>
/// Aggregate solve statistics over a player's most recent matches.
/// </summary>
public record RecentFormStatsDto(
    SolveResult? AverageSingle,
    int ValidSolves,
    int NonDnsSolves
);
