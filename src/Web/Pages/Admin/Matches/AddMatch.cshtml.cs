using BldLeague.Application.Commands.Matches.Create;
using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using BldLeague.Application.Queries.LeagueSeasons.GetLeagueSeasonsForSeasonId;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Application.Queries.Users.GetByLeagueSeasonId;
using BldLeague.Domain.Entities;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Matches;

[AdminOnly]
public class AddMatch(IMediator mediator) : PageModel
{
    [BindProperty] public CreateMatchRequest CreateMatchRequest { get; set; } = new CreateMatchRequest();
    [BindProperty] public Guid SeasonId { get; set; } = Guid.Empty;

    public IReadOnlyCollection<SeasonSummaryDto> Seasons { get; set; } = new List<SeasonSummaryDto>();
    public IReadOnlyCollection<LeagueSeasonDto> LeagueSeasons { get; set; } = new List<LeagueSeasonDto>();
    public IReadOnlyCollection<RoundSummaryDto> Rounds { get; set; } = new List<RoundSummaryDto>();
    public IReadOnlyCollection<LeagueSeasonUserDto> Users { get; set; } = new List<LeagueSeasonUserDto>();

    public async Task OnGet()
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());

        var activeSeason = Seasons.FirstOrDefault();

        if (activeSeason != null)
        {
            SeasonId = activeSeason.Id;

            LeagueSeasons = await mediator.Send(new GetLeagueSeasonsForSeasonIdRequest(SeasonId));
            var leagueSeason = LeagueSeasons.FirstOrDefault();

            if (leagueSeason != null)
            {
                CreateMatchRequest.LeagueSeasonId = leagueSeason.LeagueSeasonId;
                Users = await mediator.Send(new GetUsersByLeagueSeasonIdRequest(leagueSeason.LeagueSeasonId));
            }

            Rounds = await mediator.Send(new GetAllRoundsBySeasonIdRequest(SeasonId));
            CreateMatchRequest.RoundId = Rounds.FirstOrDefault()?.Id ?? Guid.Empty;
        }

        CreateMatchRequest.UserASolves = Enumerable.Repeat(new SolveDto(), Match.SOLVES_PER_MATCH).ToList();
        CreateMatchRequest.UserBSolves = Enumerable.Repeat(new SolveDto(), Match.SOLVES_PER_MATCH).ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());

        if (SeasonId != Guid.Empty)
        {
            LeagueSeasons = await mediator.Send(new GetLeagueSeasonsForSeasonIdRequest(SeasonId));
            Rounds = await mediator.Send(new GetAllRoundsBySeasonIdRequest(SeasonId));
        }

        if (CreateMatchRequest.LeagueSeasonId != Guid.Empty)
        {
            Users = await mediator.Send(new GetUsersByLeagueSeasonIdRequest(CreateMatchRequest.LeagueSeasonId));
        }

        if (!ModelState.IsValid)
            return Page();

        var result = await mediator.Send(CreateMatchRequest);

        if (!result.Success)
        {
            if (result.IsGeneralError)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage("/Admin/Matches/AllMatches");
            }

            ModelState.AddModelError(
                $"CreateMatchRequest.{result.Field}",
                result.Message ?? "Wystąpił błąd"
            );

            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/Matches/AllMatches");
    }

    public async Task<JsonResult> OnGetLeagues(Guid seasonId)
    {
        var leagues = await mediator.Send(new GetLeagueSeasonsForSeasonIdRequest(seasonId));
        var options = leagues.Select(ls => new { value = ls.LeagueSeasonId, text = ls.LeagueName });
        return new JsonResult(options);
    }

    public async Task<JsonResult> OnGetRounds(Guid seasonId)
    {
        var rounds = await mediator.Send(new GetAllRoundsBySeasonIdRequest(seasonId));
        var options = rounds.Select(r => new { value = r.Id, text = r.RoundName });
        return new JsonResult(options);
    }

    public async Task<JsonResult> OnGetUsers(Guid leagueSeasonId)
    {
        var users = await mediator.Send(new GetUsersByLeagueSeasonIdRequest(leagueSeasonId));
        var options = users.Select(u => new { value = u.Id, text = u.FullName });
        return new JsonResult(options);
    }
}
