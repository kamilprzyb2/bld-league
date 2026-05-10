using System.Security.Claims;
using BldLeague.Application.Commands.Matches.Submit;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Matches.GetActiveSubmission;
using BldLeague.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Submit;

[Authorize]
public class SubmitResults(IMediator mediator) : PageModel
{
    public ActiveSubmissionDto? ActiveSubmission { get; set; }

    [BindProperty]
    public List<SubmitSolveDto> Solves { get; set; } = Enumerable
        .Range(0, Match.SOLVES_PER_MATCH)
        .Select(_ => new SubmitSolveDto())
        .ToList();

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetUserId();
        ActiveSubmission = await mediator.Send(new GetActiveSubmissionRequest(userId));

        if (ActiveSubmission == null)
        {
            TempData["InfoMessage"] = "Brak aktywnego meczu.";
            return RedirectToPage("/Index");
        }

        if (ActiveSubmission.HasSubmitted)
            return RedirectToPage("/Matches/ViewMatch", new { id = ActiveSubmission.MatchId });

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = GetUserId();

        if (!ModelState.IsValid)
        {
            ActiveSubmission = await mediator.Send(new GetActiveSubmissionRequest(userId));
            return Page();
        }

        var result = await mediator.Send(new SubmitMatchSolvesRequest(userId, Solves));

        if (!result.Success)
        {
            if (result.IsGeneralError)
                TempData["ErrorMessage"] = result.Message;
            else
                ModelState.AddModelError(result.Field ?? string.Empty, result.Message ?? "Wystąpił błąd.");

            ActiveSubmission = await mediator.Send(new GetActiveSubmissionRequest(userId));
            return Page();
        }

        var submission = await mediator.Send(new GetActiveSubmissionRequest(userId));
        return RedirectToPage("/Matches/ViewMatch", new { id = submission!.MatchId });
    }
}
