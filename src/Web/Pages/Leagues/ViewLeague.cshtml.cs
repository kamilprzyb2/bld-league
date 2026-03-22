using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using BldLeague.Application.Queries.LeagueSeasons.GetDetail;
using BldLeague.Application.Queries.LeagueSeasons.GetLeagueSeasonsForSeasonId;
using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Web.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Leagues;

public class ViewLeague(IMediator mediator) : PageModel
{
    public IReadOnlyCollection<SeasonSummaryDto> Seasons { get; set; } = new List<SeasonSummaryDto>();
    public IReadOnlyCollection<LeagueSeasonDto> LeagueSeasons { get; set; } = new List<LeagueSeasonDto>();

    [BindProperty(SupportsGet = true)]
    public Guid SeasonId { get; set; } = Guid.Empty;
    [BindProperty(SupportsGet = true)]
    public Guid LeagueId { get; set; } = Guid.Empty;

    public LeagueSeasonDetailViewModel? LeagueSeason { get; set; }

    public async Task<IActionResult> OnGet()
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());

        if (!Seasons.Any())
            return Page();

        if (SeasonId == Guid.Empty)
            SeasonId = Seasons.First().Id;

        LeagueSeasons = await mediator.Send(new GetLeagueSeasonsForSeasonIdRequest(SeasonId));

        if (!LeagueSeasons.Any())
            return Page();

        if (LeagueId == Guid.Empty || !LeagueSeasons.Any(ls => ls.LeagueId == LeagueId))
            LeagueId = LeagueSeasons.First().LeagueId;

        var dto = await mediator.Send(new GetLeagueSeasonDetailRequest(SeasonId, LeagueId));
        LeagueSeason = dto == null ? null : LeagueSeasonDetailViewModel.FromDto(dto);

        ModelState.Clear();
        return Page();
    }
}
