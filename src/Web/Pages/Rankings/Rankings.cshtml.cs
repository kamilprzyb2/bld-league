using BldLeague.Application.Queries.PlayerRankings.GetPlayerRankings;
using BldLeague.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Rankings;

public class Rankings(IMediator mediator) : PageModel
{
    public IReadOnlyCollection<SingleRankingDto> SingleRankings { get; set; } = new List<SingleRankingDto>();
    public IReadOnlyCollection<AverageRankingDto> AverageRankings { get; set; } = new List<AverageRankingDto>();

    public async Task OnGet()
    {
        var response = await mediator.Send(new GetPlayerRankingsRequest());
        SingleRankings = response.SingleRankings;
        AverageRankings = response.AverageRankings;
    }

    public static string FormatSolveWithParens(AverageRankingDto dto, int solveIndex)
    {
        var solves = new[] { dto.Solve1, dto.Solve2, dto.Solve3, dto.Solve4, dto.Solve5 };
        var solve = solves[solveIndex];

        // Determine best (min valid) and worst (first DNF/DNS if any, else max valid)
        var validSolves = solves.Where(s => s.IsValid).ToList();
        bool isBest = false;
        bool isWorst = false;

        if (validSolves.Count > 0)
        {
            int minCs = validSolves.Min(s => s.Centiseconds);
            // best is lowest valid
            if (solve.IsValid && solve.Centiseconds == minCs)
            {
                // mark as best only for the first occurrence of this value
                bool foundEarlier = false;
                for (int i = 0; i < solveIndex; i++)
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

        // worst: first DNF/DNS if any, else first max valid
        bool hasDnfOrDns = solves.Any(s => !s.IsValid);
        if (hasDnfOrDns)
        {
            // worst = first DNF or DNS
            for (int i = 0; i < solves.Length; i++)
            {
                if (!solves[i].IsValid)
                {
                    if (i == solveIndex) isWorst = true;
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
                    if (i == solveIndex) isWorst = true;
                    break;
                }
            }
        }

        string display = solve.ToString();
        if (isBest || isWorst) return $"({display})";
        return display;
    }
}
