using BldLeague.Application.Commands.Matches.Edit;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Domain.Entities;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Matches;

[AdminOnly]
public class EditMatch(IMediator mediator) : PageModel
{
    [BindProperty] public EditMatchRequest EditMatchRequest { get; set; } = new();

    public MatchDetailsDto? MatchDetails { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        MatchDetails = await mediator.Send(new GetMatchDetailsByIdRequest(id));

        if (MatchDetails == null)
            return NotFound();

        EditMatchRequest.MatchId = MatchDetails.Id;
        EditMatchRequest.UserASolves = MatchDetails.SolvesUserA
            .Select(s => new SolveDto { Result = s.ToString() })
            .ToList();
        EditMatchRequest.UserBSolves = MatchDetails.SolvesUserB
            .Select(s => new SolveDto { Result = s.ToString() })
            .ToList();

        // Pad UserB solves to SOLVES_PER_MATCH in case of a bye
        while (EditMatchRequest.UserBSolves.Count < Match.SOLVES_PER_MATCH)
            EditMatchRequest.UserBSolves.Add(new SolveDto());

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            MatchDetails = await mediator.Send(new GetMatchDetailsByIdRequest(EditMatchRequest.MatchId));
            return Page();
        }

        var result = await mediator.Send(EditMatchRequest);

        if (!result.Success)
        {
            if (result.IsGeneralError)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage("/Admin/Matches/AllMatches");
            }

            MatchDetails = await mediator.Send(new GetMatchDetailsByIdRequest(EditMatchRequest.MatchId));

            ModelState.AddModelError(
                $"EditMatchRequest.{result.Field}",
                result.Message ?? "Wystąpił błąd"
            );

            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/Matches/AllMatches");
    }
}
