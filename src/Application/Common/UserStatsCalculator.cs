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

        SolveResult? averageSingle = CalculateMean(validSolves);
        SolveResult? medianSingle = CalculateMedian(nonDnsSolves);

        int longestSuccessStreak = StreakCalculator.LongestSuccessStreak(solves);

        // Matches vs real opponents (exclude BYE), in chronological order
        var vsOpponent = matchesNewestFirst.Where(m => m.OpponentFullName != null).Reverse().ToList();
        int wins = vsOpponent.Count(m => m.SelfScore > m.OpponentScore);
        int losses = vsOpponent.Count(m => m.SelfScore < m.OpponentScore);
        int draws = vsOpponent.Count(m => m.SelfScore == m.OpponentScore);

        int longestWinStreak = StreakCalculator.LongestWinStreak(
            vsOpponent.Select(m => (m.SelfScore, m.OpponentScore)));

        return new UserStats(validSolves.Count, nonDnsSolves.Count, averageSingle, medianSingle, wins, losses, draws, longestSuccessStreak, longestWinStreak);
    }

    /// <summary>
    /// Arithmetic mean of the valid solves.
    /// </summary>
    public static SolveResult? CalculateMean(IReadOnlyCollection<SolveResult> validSolves)
        => validSolves.Count > 0
            ? SolveResult.FromCentiseconds((int)Math.Round(validSolves.Average(s => (double)s.Centiseconds)))
            : null;

    /// <summary>
    /// Median of the non-DNS solves, with DNF counting as slower than any time —
    /// more than half DNFs yields a DNF median. For an even count, the mean of the
    /// two middle values (DNF if either middle value is a DNF).
    /// </summary>
    public static SolveResult? CalculateMedian(IReadOnlyCollection<SolveResult> nonDnsSolves)
    {
        if (nonDnsSolves.Count == 0)
            return null;

        var sorted = nonDnsSolves
            .Select(s => s.IsValid ? s.Centiseconds : int.MaxValue)
            .OrderBy(c => c)
            .ToList();
        int middle = sorted.Count / 2;

        if (sorted.Count % 2 != 0)
            return sorted[middle] == int.MaxValue
                ? SolveResult.Dnf()
                : SolveResult.FromCentiseconds(sorted[middle]);

        return sorted[middle] == int.MaxValue
            ? SolveResult.Dnf()
            : SolveResult.FromCentiseconds((int)Math.Round((sorted[middle - 1] + sorted[middle]) / 2.0));
    }
}
