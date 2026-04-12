using BldLeague.Domain.ValueObjects;

namespace BldLeague.Web.Helpers;

public static class SolveFormatHelper
{
    /// <summary>
    /// Formats a single solve from a set of 5, wrapping it in parentheses if it is the best or worst
    /// (following WCA Ao5 convention: best = lowest valid, worst = first DNF/DNS or first max valid).
    /// </summary>
    public static string FormatWithParens(SolveResult[] solves, int index)
    {
        var solve = solves[index];

        var validSolves = solves.Where(s => s.IsValid).ToList();
        bool isBest = false;
        bool isWorst = false;

        if (validSolves.Count > 0)
        {
            int minCs = validSolves.Min(s => s.Centiseconds);
            if (solve.IsValid && solve.Centiseconds == minCs)
            {
                bool foundEarlier = false;
                for (int i = 0; i < index; i++)
                {
                    if (solves[i].IsValid && solves[i].Centiseconds == minCs)
                    {
                        foundEarlier = true;
                        break;
                    }
                }
                if (!foundEarlier) isBest = true;
            }
        }

        bool hasDnfOrDns = solves.Any(s => !s.IsValid);
        if (hasDnfOrDns)
        {
            for (int i = 0; i < solves.Length; i++)
            {
                if (!solves[i].IsValid)
                {
                    if (i == index) isWorst = true;
                    break;
                }
            }
        }
        else if (validSolves.Count > 0)
        {
            int maxCs = validSolves.Max(s => s.Centiseconds);
            for (int i = 0; i < solves.Length; i++)
            {
                if (solves[i].IsValid && solves[i].Centiseconds == maxCs)
                {
                    if (i == index) isWorst = true;
                    break;
                }
            }
        }

        string display = solve.ToString();
        return (isBest || isWorst) ? $"({display})" : display;
    }
}
