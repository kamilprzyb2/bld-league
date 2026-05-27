using BldLeague.Domain.ValueObjects;

namespace BldLeague.Web.Helpers;

public static class SolveFormatHelper
{
    /// <summary>
    /// Formats a single solve from a set, wrapping it in parentheses if it is among the
    /// <paramref name="dropCount"/> best or <paramref name="dropCount"/> worst (following WCA convention:
    /// best = lowest valid times, worst = invalid solves first then highest valid times;
    /// ties broken by first occurrence).
    /// </summary>
    public static string FormatWithParens(SolveResult[] solves, int index, int dropCount = 1)
    {
        var solve = solves[index];

        bool isBest = false;
        if (solve.IsValid)
        {
            int rankFromBest = 0;
            for (int i = 0; i < solves.Length; i++)
            {
                if (i == index || !solves[i].IsValid) continue;
                if (solves[i].Centiseconds < solve.Centiseconds ||
                    (solves[i].Centiseconds == solve.Centiseconds && i < index))
                    rankFromBest++;
            }
            isBest = rankFromBest < dropCount;
        }

        int rankFromWorst = 0;
        for (int i = 0; i < solves.Length; i++)
        {
            if (i == index) continue;
            bool iWorse;
            if (!solves[i].IsValid && !solve.IsValid)
                iWorse = i < index;
            else if (!solves[i].IsValid)
                iWorse = true;
            else if (!solve.IsValid)
                iWorse = false;
            else
                iWorse = solves[i].Centiseconds > solve.Centiseconds ||
                         (solves[i].Centiseconds == solve.Centiseconds && i < index);
            if (iWorse) rankFromWorst++;
        }
        bool isWorst = rankFromWorst < dropCount;

        string display = solve.ToString();
        return (isBest || isWorst) ? $"({display})" : display;
    }
}
