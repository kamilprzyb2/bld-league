using BldLeague.Application.Commands.Leagues.Update;
using BldLeague.Application.Queries.Leagues.GetById;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Leagues;

[AdminOnly]
public class EditLeague(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    [BindProperty]
    public UpdateLeagueRequest UpdateLeagueRequest { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        var league = await mediator.Send(new GetLeagueByIdRequest { LeagueId = Id });

        if (league == null)
        {
            TempData["ErrorMessage"] = $"Nie znaleziono ligi z id: {Id}.";
            return RedirectToPage("/Admin/Leagues/AllLeagues");
        }

        UpdateLeagueRequest = new UpdateLeagueRequest
        {
            LeagueId = league.Id,
            LeagueIdentifier = league.LeagueIdentifier
        };
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await mediator.Send(UpdateLeagueRequest);

        if (!result.Success)
        {
            if (result.IsGeneralError)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                ModelState.AddModelError($"UpdateLeagueRequest.{result.Field}", result.Message);
                return Page();
            }
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/Leagues/AllLeagues");
    }
}
