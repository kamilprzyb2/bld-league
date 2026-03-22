using BldLeague.Application.Queries.Leagues.GetAll;
using BldLeague.Application.Queries.Matches.GetMatchSummaries;
using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Web.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Matches;

public class MatchList(IMediator mediator) : PageModel
{
    public IReadOnlyCollection<SeasonSummaryDto> Seasons { get; set; } = new List<SeasonSummaryDto>();
    public IReadOnlyCollection<LeagueSummaryDto> Leagues { get; set; } = new List<LeagueSummaryDto>();
    public IReadOnlyCollection<RoundSummaryDto> Rounds { get; set; } = new List<RoundSummaryDto>();

    [BindProperty(SupportsGet = true)]
    public Guid SeasonId { get; set; } = Guid.Empty;
    [BindProperty(SupportsGet = true)]
    public Guid LeagueId { get; set; } = Guid.Empty;
    [BindProperty(SupportsGet = true)]
    public int RoundNumber { get; set; } = 0;

    public IReadOnlyCollection<MatchSummaryViewModel> MatchSummaries { get; set; } = new List<MatchSummaryViewModel>();

    public async Task<IActionResult> OnGet()
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());
        Leagues = await mediator.Send(new GetAllLeaguesRequest());

        if (!Seasons.Any() || !Leagues.Any())
            return Page();

        if (SeasonId == Guid.Empty)
            SeasonId = Seasons.First().Id;

        if (LeagueId == Guid.Empty || !Leagues.Any(l => l.Id == LeagueId))
            LeagueId = Leagues.First().Id;

        Rounds = await mediator.Send(new GetAllRoundsBySeasonIdRequest(SeasonId));

        if (Rounds.Any() && (RoundNumber == 0 || !Rounds.Any(r => r.RoundNumber == RoundNumber)))
            RoundNumber = Rounds.GetDefaultRound(DateTime.Today).RoundNumber;

        var dtos = await mediator.Send(new GetMatchSummariesRequest(SeasonId, LeagueId, RoundNumber));
        MatchSummaries = dtos.Select(MatchSummaryViewModel.FromDto).ToList();

        ModelState.Clear();
        return Page();
    }
}
