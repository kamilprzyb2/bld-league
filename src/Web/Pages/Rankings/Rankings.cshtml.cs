using BldLeague.Application.Queries.PlayerRankings.GetPlayerRankings;
using BldLeague.Web.Helpers;
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
        return SolveFormatHelper.FormatWithParens(solves, solveIndex);
    }
}
