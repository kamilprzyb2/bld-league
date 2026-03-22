using BldLeague.Application.Commands.Seasons.Edit;
using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Seasons;

[AdminOnly]
public class EditSeason(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty] public EditSeasonRequest EditSeasonRequest { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        var season = (await mediator.Send(new GetAllSeasonsRequest()))
            .FirstOrDefault(s => s.Id == Id);

        if (season == null)
        {
            TempData["ErrorMessage"] = $"Nie znaleziono sezonu z ID {Id}";
            return RedirectToPage("/Admin/Seasons/AllSeasons");
        }

        EditSeasonRequest = new EditSeasonRequest { SeasonId = season.Id, SeasonNumber = season.SeasonNumber };
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await mediator.Send(EditSeasonRequest);

        if (!result.Success)
        {
            if (result.IsGeneralError)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage("/Admin/Seasons/AllSeasons");
            }
            ModelState.AddModelError($"EditSeasonRequest.{result.Field}", result.Message ?? "Wystąpił błąd");
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/Seasons/AllSeasons");
    }
}
