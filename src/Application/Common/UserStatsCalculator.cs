using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Common;

/// <summary>
/// Computes aggregate career statistics shared by the user profile and the head-to-head comparison.
/// </summary>
public static class UserStatsCalculator
{
    /// <summary>
    /// Computes stats from a user's finished solves (chronological order) and match history (newest first).
    /// </summary>
    public static UserStats Calculate(
        IReadOnlyCollection<SolveResult> solves,
        IReadOnlyCollection<MatchPerspective> matchesNewestFirst)
    {
        var nonDnsSolves = solves.Where(s => !s.IsDns).ToList();
        var validSolves = nonDnsSolves.Where(s => s.IsValid).ToList();

        SolveResult? averageSingle = validSolves.Count > 0
            ? SolveResult.FromCentiseconds((int)Math.Round(validSolves.Average(s => (double)s.Centiseconds)))
            : null;

        int longestSuccessStreak = StreakCalculator.LongestSuccessStreak(solves);

        // Matches vs real opponents (exclude BYE), in chronological order
        var vsOpponent = matchesNewestFirst.Where(m => m.OpponentFullName != null).Reverse().ToList();
        int wins = vsOpponent.Count(m => m.SelfScore > m.OpponentScore);
        int losses = vsOpponent.Count(m => m.SelfScore < m.OpponentScore);
        int draws = vsOpponent.Count(m => m.SelfScore == m.OpponentScore);

        int longestWinStreak = StreakCalculator.LongestWinStreak(
            vsOpponent.Select(m => (m.SelfScore, m.OpponentScore)));

        return new UserStats(validSolves.Count, nonDnsSolves.Count, averageSingle, wins, losses, draws, longestSuccessStreak, longestWinStreak);
    }
}
