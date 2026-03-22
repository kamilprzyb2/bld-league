using BldLeague.Application.Commands.Rounds.Create;
using BldLeague.Application.Queries.Rounds.GetMaxRoundNumberBySeasonId;
using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Rounds;

[AdminOnly]
public class AddRound(IMediator mediator) : PageModel
{
    public IReadOnlyCollection<SeasonSummaryDto> Seasons { get; private set; } = Array.Empty<SeasonSummaryDto>();
    [BindProperty] public CreateRoundRequest CreateRoundRequest { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());
        if (Seasons.Count > 0)
        {
            var activeSeason = Seasons.First(); // ordered by SeasonNumber desc — first = latest
            CreateRoundRequest.SeasonId = activeSeason.Id;
            var maxRound = await mediator.Send(new GetMaxRoundNumberBySeasonIdRequest(activeSeason.Id));
            CreateRoundRequest.RoundNumber = maxRound.HasValue ? maxRound.Value + 1 : 1;
        }
        CreateRoundRequest.StartDate = DateTime.Now;
        CreateRoundRequest.EndDate = DateTime.Now.AddDays(1);
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());

        if (!ModelState.IsValid)
            return Page();

        CreateRoundRequest.StartDate = CreateRoundRequest.StartDate.ToUniversalTime();
        CreateRoundRequest.EndDate = CreateRoundRequest.EndDate.ToUniversalTime();

        var result = await mediator.Send(CreateRoundRequest);

        if (!result.Success)
        {
            if (result.IsGeneralError)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage("/Admin/Rounds/AllRounds");
            }
            ModelState.AddModelError(
                $"CreateRoundRequest.{result.Field}",
                result.Message ?? "Wystąpił błąd"
            );
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/Rounds/AllRounds");
    }
}
