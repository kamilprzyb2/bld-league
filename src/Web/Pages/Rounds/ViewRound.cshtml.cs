using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using BldLeague.Application.Queries.Rounds.GetDetail;
using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Web.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Rounds;

public class ViewRound(IMediator mediator) : PageModel
{
    public IReadOnlyCollection<SeasonSummaryDto> Seasons { get; set; } = new List<SeasonSummaryDto>();
    public IReadOnlyCollection<RoundSummaryDto> Rounds { get; set; } = new List<RoundSummaryDto>();

    [BindProperty(SupportsGet = true)]
    public Guid SeasonId { get; set; } = Guid.Empty;
    [BindProperty(SupportsGet = true)]
    public int RoundNumber { get; set; } = 0;

    public RoundDetailViewModel? RoundDetail { get; set; }

    public async Task<IActionResult> OnGet()
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());

        if (!Seasons.Any())
            return Page();

        if (SeasonId == Guid.Empty)
            SeasonId = Seasons.First().Id;

        Rounds = await mediator.Send(new GetAllRoundsBySeasonIdRequest(SeasonId));

        if (!Rounds.Any())
            return Page();

        if (RoundNumber == 0 || !Rounds.Any(r => r.RoundNumber == RoundNumber))
            RoundNumber = Rounds.GetDefaultRound(DateTime.Today).RoundNumber;

        var dto = await mediator.Send(new GetRoundDetailRequest(SeasonId, RoundNumber));
        RoundDetail = dto == null ? null : RoundDetailViewModel.FromDto(dto);

        ModelState.Clear();
        return Page();
    }
}
