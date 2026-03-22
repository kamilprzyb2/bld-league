using BldLeague.Application.Commands.Seasons.Create;
using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Seasons;

[AdminOnly]
public class AddSeason(IMediator mediator) : PageModel
{
    [BindProperty] public CreateSeasonRequest CreateSeasonRequest { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        var latestNumber = (await mediator.Send(new GetAllSeasonsRequest()))
            .Select(s => s.SeasonNumber)
            .DefaultIfEmpty(0)
            .Max();
        CreateSeasonRequest.SeasonNumber = latestNumber + 1;
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await mediator.Send(CreateSeasonRequest);

        if (!result.Success)
        {
            if (result.IsGeneralError)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage("/Admin/Seasons/AllSeasons");
            }
            ModelState.AddModelError($"CreateSeasonRequest.{result.Field}", result.Message ?? "Wystąpił błąd");
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/Seasons/AllSeasons");
    }
}
