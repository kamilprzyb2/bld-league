using BldLeague.Application.Queries.Users.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Users;

public class UserList(IMediator mediator) : PageModel
{
    public IReadOnlyCollection<UserSummaryDto> Users { get; set; } = [];

    public async Task OnGet()
    {
        Users = await mediator.Send(new GetAllUsersRequest());
    }
}
