using BldLeague.Application.Commands.Rounds.Update;
using BldLeague.Application.Commands.Rounds.UpdateScrambles;
using BldLeague.Application.Queries.Rounds.GetById;
using BldLeague.Application.Queries.Rounds.GetScrambles;
using BldLeague.Domain.Entities;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Rounds;

[AdminOnly]
public class EditRound(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty] public UpdateRoundRequest UpdateRoundRequest { get; set; } = new();
    [BindProperty] public string?[] ScrambleNotations { get; set; } = new string?[Match.SOLVES_PER_MATCH];

    public async Task<IActionResult> OnGet()
    {
        var round = await mediator.Send(new GetRoundByIdRequest(Id));

        if (round == null)
        {
            TempData["ErrorMessage"] = $"Nie znaleziono kolejki z ID {Id}";
            return RedirectToPage("/Admin/Rounds/AllRounds");
        }

        UpdateRoundRequest = new UpdateRoundRequest
        {
            Id = round.Id,
            RoundNumber = round.RoundNumber,
            StartDate = round.StartDate,
            EndDate = round.EndDate,
            SubmissionFormUrl = round.SubmissionFormUrl,
        };

        var scrambles = await mediator.Send(new GetScramblesByRoundIdRequest(round.Id));
        ScrambleNotations = new string?[Match.SOLVES_PER_MATCH];
        foreach (var s in scrambles)
            ScrambleNotations[s.ScrambleNumber - 1] = s.Notation;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var round = await mediator.Send(new GetRoundByIdRequest(Id));

        if (round == null)
        {
            TempData["ErrorMessage"] = $"Nie znaleziono kolejki z ID {Id}";
            return RedirectToPage("/Admin/Rounds/AllRounds");
        }

        if (!ModelState.IsValid)
            return Page();

        UpdateRoundRequest.StartDate = UpdateRoundRequest.StartDate.ToUniversalTime();
        UpdateRoundRequest.EndDate = UpdateRoundRequest.EndDate.ToUniversalTime();

        var result = await mediator.Send(UpdateRoundRequest);

        if (!result.Success)
        {
            if (result.IsGeneralError)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage("/Admin/Rounds/AllRounds");
            }
            ModelState.AddModelError(
                $"UpdateRoundRequest.{result.Field}",
                result.Message ?? "Wystąpił błąd"
            );
            return Page();
        }

        var scrambleResult = await mediator.Send(new UpdateRoundScramblesRequest
        {
            RoundId = Id,
            Notations = ScrambleNotations ?? new string?[Match.SOLVES_PER_MATCH]
        });

        if (!scrambleResult.Success)
        {
            TempData["ErrorMessage"] = scrambleResult.Message;
            return RedirectToPage("/Admin/Rounds/AllRounds");
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/Rounds/AllRounds");
    }
}
