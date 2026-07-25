using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Common;

/// <summary>
/// Aggregate career statistics for a single user.
/// </summary>
public record UserStats(
    int ValidSolves,
    int NonDnsSolves,
    SolveResult? AverageSingle,
    int Wins,
    int Losses,
    int Draws,
    int LongestSuccessStreak,
    int LongestWinStreak
);
