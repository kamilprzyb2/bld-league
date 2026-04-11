using BldLeague.Application.Commands.PlayerRankings.Refresh;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin;

[AdminOnly]
public class Admin(IMediator mediator) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostRefreshRankings()
    {
        var result = await mediator.Send(new RefreshPlayerRankingsRequest());
        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }
}