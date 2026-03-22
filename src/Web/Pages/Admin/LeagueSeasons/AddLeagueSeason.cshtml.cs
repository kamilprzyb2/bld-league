using BldLeague.Application.Commands.LeagueSeasons.Create;
using BldLeague.Application.Queries.Leagues.GetAll;
using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.LeagueSeasons;

[AdminOnly]
public class AddLeagueSeason(IMediator mediator) : PageModel
{
    [BindProperty] public CreateLeagueSeasonRequest CreateLeagueSeasonRequest { get; set; } = new();

    public IReadOnlyCollection<SeasonSummaryDto> Seasons { get; private set; } = Array.Empty<SeasonSummaryDto>();
    public IReadOnlyCollection<LeagueSummaryDto> Leagues { get; private set; } = Array.Empty<LeagueSummaryDto>();

    public async Task OnGet()
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());
        Leagues = await mediator.Send(new GetAllLeaguesRequest());
    }

    public async Task<IActionResult> OnPost()
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());
        Leagues = await mediator.Send(new GetAllLeaguesRequest());

        if (!ModelState.IsValid)
            return Page();

        var result = await mediator.Send(CreateLeagueSeasonRequest);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/LeagueSeasons/AllLeagueSeasons");
    }
}
