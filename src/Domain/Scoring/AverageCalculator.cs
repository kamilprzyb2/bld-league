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
}