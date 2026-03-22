using BldLeague.Application.Commands.Leagues.Create;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Leagues;

[AdminOnly]
public class AddLeague(IMediator mediator) : PageModel
{
    [BindProperty]
    public CreateLeagueRequest CreateLeagueRequest { get; set; } = new();
    
    public void OnGet()
    {
        
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();
        
        var result = await mediator.Send(CreateLeagueRequest);
        
        if (!result.Success)
        {
            ModelState.AddModelError(
                $"CreateUserRequest.{result.Field}",
                result.Message ?? "Wystąpił błąd"
            );
            return Page();
        }
        
        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/Leagues/AllLeagues");
    }
}