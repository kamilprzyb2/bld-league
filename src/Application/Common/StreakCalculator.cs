using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Common;

/// <summary>
/// Shared streak computations used by user profile stats and the global statistics page.
/// </summary>
public static class StreakCalculator
{
    /// <summary>
    /// Computes the longest run of consecutive valid (not DNF, not DNS) solves.
    /// DNS solves are skipped — they neither extend nor break the streak.
    /// DNF solves break the current streak.
    /// </summary>
    /// <param name="solvesInOrder">Solves in chronological order.</param>
    public static int LongestSuccessStreak(IEnumerable<SolveResult> solvesInOrder)
    {
        int longest = 0, current = 0;
        foreach (var s in solvesInOrder)
        {
            if (s.IsDns) continue;
            if (s.IsValid)
            {
                current++;
                if (current > longest) longest = current;
            }
            else
            {
                current = 0;
            }
        }
        return longest;
    }

    /// <summary>
    /// Computes the longest run of consecutive match wins.
    /// A win is when <c>self &gt; opponent</c>; a draw or a loss breaks the streak.
    /// </summary>
    /// <param name="matchesInOrder">Match results vs a real opponent, in chronological order. BYE matches must be excluded by the caller.</param>
    public static int LongestWinStreak(IEnumerable<(int Self, int Opponent)> matchesInOrder)
    {
        int longest = 0, current = 0;
        foreach (var (self, opponent) in matchesInOrder)
        {
            if (self > opponent)
            {
                current++;
                if (current > longest) longest = current;
            }
            else
            {
                current = 0;
            }
        }
        return longest;
    }
}
