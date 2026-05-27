using BldLeague.Domain.ValueObjects;

namespace BldLeague.Domain.Scoring;

/// <summary>
/// Provides methods to calculate averages for a set of solves, e.g., Ao5.
/// </summary>
public class AverageCalculator
{
    /// <summary>
    /// Calculates the average of 5 solves according to WCA rules:
    /// - Throws <see cref="ArgumentException"/> if the number of solves is not exactly 5.
    /// - If more than 1 solve is invalid (DNF or DNS), returns DNF.
    /// - Drops the best and worst valid solves, and returns the arithmetic mean of the remaining 3.
    /// </summary>
    /// <param name="solves">Collection of 5 <see cref="SolveResult"/> instances.</param>
    /// <returns>
    /// A <see cref="SolveResult"/> representing the average:
    /// - Valid average in centiseconds if calculable.
    /// - DNS if all 5 solves are DNS (entire set was not started).
    /// - DNF otherwise if more than 1 invalid solve (even a mix of DNF and DNS).
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="solves"/> does not contain exactly 5 elements.</exception>
    public static SolveResult CalculateAo5(List<SolveResult> solves)
    {
        if (solves.Count != 5)
            throw new ArgumentException($"Ao5 requires exactly 5 solves. Provided: {solves.Count}");

        // Count invalid solves (DNF or DNS)
        var invalidCount = solves.Count(s => !s.IsValid);
        if (invalidCount > 1)
            return solves.All(s => s.IsDns) ? SolveResult.Dns() : SolveResult.Dnf();

        // Sort by centiseconds, treating DNFs as max
        var sorted = solves
            .OrderBy(s => s.IsValid ? s.Centiseconds : int.MaxValue)
            .ToList();

        // Drop best and worst (first = best, last = worst)
        var middleThree = sorted.Skip(1).Take(3).ToList();

        // If any of the middle three is DNF, the average is DNF
        if (middleThree.Any(s => !s.IsValid))
            return SolveResult.Dnf();

        // Compute arithmetic mean
        var averageCs = (int)Math.Round(middleThree.Average(s => s.Centiseconds));
        return SolveResult.FromCentiseconds(averageCs);
    }

    /// <summary>
    /// Calculates the average of 12 solves according to WCA rules:
    /// - Throws <see cref="ArgumentException"/> if the number of solves is not exactly 12.
    /// - If 2 or more solves are invalid (DNF or DNS), returns DNF (or DNS if all 12 are DNS).
    /// - Drops the best and worst valid solves, and returns the arithmetic mean of the remaining 10.
    /// </summary>
    /// <param name="solves">Collection of 12 <see cref="SolveResult"/> instances.</param>
    /// <returns>
    /// A <see cref="SolveResult"/> representing the average:
    /// - Valid average in centiseconds if calculable.
    /// - DNS if all 12 solves are DNS (entire set was not started).
    /// - DNF otherwise if more than 1 invalid solve (even a mix of DNF and DNS), or if any of the middle 10 is DNF.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="solves"/> does not contain exactly 12 elements.</exception>
    public static SolveResult CalculateAo12(List<SolveResult> solves)
    {
        if (solves.Count != 12)
            throw new ArgumentException($"Ao12 requires exactly 12 solves. Provided: {solves.Count}");

        // Count invalid solves (DNF or DNS)
        var invalidCount = solves.Count(s => !s.IsValid);
        if (invalidCount > 1)
            return solves.All(s => s.IsDns) ? SolveResult.Dns() : SolveResult.Dnf();

        // Sort by centiseconds, treating DNFs as max
        var sorted = solves
            .OrderBy(s => s.IsValid ? s.Centiseconds : int.MaxValue)
            .ToList();

        // Drop best and worst (first = best, last = worst)
        var middleTen = sorted.Skip(1).Take(10).ToList();

        // If any of the middle ten is DNF, the average is DNF
        if (middleTen.Any(s => !s.IsValid))
            return SolveResult.Dnf();

        // Compute arithmetic mean
        var averageCs = (int)Math.Round(middleTen.Average(s => s.Centiseconds));
        return SolveResult.FromCentiseconds(averageCs);
    }

    /// <summary>
    /// Calculates the average of 25 solves following the same drop-extremes pattern as Ao5 / Ao12:
    /// - Throws <see cref="ArgumentException"/> if the number of solves is not exactly 25.
    /// - If 3 or more solves are invalid (DNF or DNS), returns DNF (or DNS if all 25 are DNS).
    /// - Drops the 2 best and 2 worst valid solves, and returns the arithmetic mean of the remaining 21.
    /// </summary>
    /// <param name="solves">Collection of 25 <see cref="SolveResult"/> instances.</param>
    /// <returns>
    /// A <see cref="SolveResult"/> representing the average:
    /// - Valid average in centiseconds if calculable.
    /// - DNS if all 25 solves are DNS (entire set was not started).
    /// - DNF otherwise if 3 or more invalid solves (even a mix of DNF and DNS), or if any of the middle 21 is DNF.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="solves"/> does not contain exactly 25 elements.</exception>
    public static SolveResult CalculateAo25(List<SolveResult> solves)
    {
        if (solves.Count != 25)
            throw new ArgumentException($"Ao25 requires exactly 25 solves. Provided: {solves.Count}");

        // Count invalid solves (DNF or DNS)
        var invalidCount = solves.Count(s => !s.IsValid);
        if (invalidCount >= 3)
            return solves.All(s => s.IsDns) ? SolveResult.Dns() : SolveResult.Dnf();

        // Sort by centiseconds, treating DNFs as max
        var sorted = solves
            .OrderBy(s => s.IsValid ? s.Centiseconds : int.MaxValue)
            .ToList();

        // Drop 2 best and 2 worst (first two = best, last two = worst)
        var middleTwentyOne = sorted.Skip(2).Take(21).ToList();

        // If any of the middle twenty-one is DNF, the average is DNF
        if (middleTwentyOne.Any(s => !s.IsValid))
            return SolveResult.Dnf();

        // Compute arithmetic mean
        var averageCs = (int)Math.Round(middleTwentyOne.Average(s => s.Centiseconds));
        return SolveResult.FromCentiseconds(averageCs);
    }
}