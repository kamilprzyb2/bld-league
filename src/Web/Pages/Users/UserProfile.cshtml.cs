using BldLeague.Application.Queries.PlayerRankings.GetByUserId;
using BldLeague.Application.Queries.Users.GetAll;
using BldLeague.Application.Queries.Users.GetById;
using BldLeague.Application.Queries.Users.GetMatchHistory;
using BldLeague.Application.Queries.Users.GetRoundResults;
using BldLeague.Application.Queries.Users.GetSeasonHistory;
using BldLeague.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Users;

public class UserProfile(IMediator mediator) : PageModel
{
    public UserSummaryDto? Profile { get; set; }
    public PlayerRankingDto? Ranking { get; set; }
    public IReadOnlyCollection<UserRoundResultDto> RoundResults { get; set; } = [];
    public IReadOnlyCollection<UserMatchHistoryDto> MatchHistory { get; set; } = [];
    public IReadOnlyCollection<UserSeasonHistoryDto> SeasonHistory { get; set; } = [];

    public async Task<IActionResult> OnGet(Guid id)
    {
        Profile = await mediator.Send(new GetUserByIdRequest { UserId = id });
        if (Profile == null)
            return NotFound();

        Ranking = await mediator.Send(new GetPlayerRankingByUserIdRequest { UserId = id });
        RoundResults = await mediator.Send(new GetUserRoundResultsRequest { UserId = id });
        MatchHistory = await mediator.Send(new GetUserMatchHistoryRequest { UserId = id });
        SeasonHistory = await mediator.Send(new GetUserSeasonHistoryRequest { UserId = id });

        return Page();
    }

    public static string FormatSolveWithParens(UserRoundResultDto result, int index)
    {
        var solves = new[] { result.Solve1, result.Solve2, result.Solve3, result.Solve4, result.Solve5 };
        return SolveFormatHelper.FormatWithParens(solves, index);
    }
}
