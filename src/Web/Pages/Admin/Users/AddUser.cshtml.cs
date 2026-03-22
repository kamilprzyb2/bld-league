using BldLeague.Application.Commands.Users.Create;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Users;

[AdminOnly]
public class AddUser(IMediator mediator) : PageModel
{
    [BindProperty]
    public CreateUserRequest CreateUserRequest { get; set; } = new();
    
    public void OnGet()
    {
        
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();
        
        var result = await mediator.Send(CreateUserRequest);
        
        if (!result.Success)
        {
            ModelState.AddModelError(
                $"CreateUserRequest.{result.Field}",
                result.Message ?? "Wystąpił błąd"
            );
            return Page();
        }
        
        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/Users/AllUsers");
    }
}