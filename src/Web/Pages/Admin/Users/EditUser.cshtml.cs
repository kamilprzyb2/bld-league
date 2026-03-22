using BldLeague.Application.Commands.Users.Update;
using BldLeague.Application.Queries.Users.GetById;
using BldLeague.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Users;

[AdminOnly]
public class EditUser(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    [BindProperty]
    public UpdateUserRequest UpdateUserRequest { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        var user = await mediator.Send(new GetUserByIdRequest { UserId = Id });

        if (user == null)
        {
            TempData["ErrorMessage"] = $"Nie znaleziono użytkownika z id: {Id}.";
            return RedirectToPage("/Admin/Users/AllUsers");
        }

        UpdateUserRequest = new UpdateUserRequest
        {
            FullName = user.FullName,
            WcaId = user.WcaId,
            AvatarImageUrl = user.AvatarUrl,
            IsAdmin = user.IsAdmin,
            UserId = user.Id
        };
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await mediator.Send(UpdateUserRequest);

        if (!result.Success)
        {
            if (result.IsGeneralError)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                ModelState.AddModelError($"UpdateUserRequest.{result.Field}", result.Message);
                return Page();
            }
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Admin/Users/AllUsers");
    }
}
