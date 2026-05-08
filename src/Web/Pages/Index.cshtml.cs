using System.Security.Claims;
using BldLeague.Application.Queries.Matches.GetActiveSubmission;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages;

public class IndexModel(IMediator mediator) : PageModel
{
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
    public string? ThumbnailUrl { get; set; }
    public ActiveSubmissionDto? ActiveSubmission { get; set; }

    public async Task OnGet()
    {
        IsAuthenticated = User.Identity?.IsAuthenticated == true;
        if (IsAuthenticated)
        {
            UserName = User.FindFirst(ClaimTypes.Name)?.Value;
            Role = User.FindFirst(ClaimTypes.Role)?.Value;
            ThumbnailUrl = User.FindFirst("thumbnail")?.Value;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                ActiveSubmission = await mediator.Send(new GetActiveSubmissionRequest(userId));
            }
        }
    }
}
