using System.Security.Claims;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Web.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Matches;

public class ViewMatch(IMediator mediator, RoundClock roundClock) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public MatchDetailsViewModel? MatchDetails { get; set; }

    /// <summary>
    /// Local-time string of when the current viewer submitted their results, or null if not a participant / not yet submitted.
    /// </summary>
    public string? ViewerSubmittedAt { get; set; }

    public async Task OnGet()
    {
        var dto = await mediator.Send(new GetMatchDetailsByIdRequest(Id));
        if (dto == null)
            return;

        MatchDetails = MatchDetailsViewModel.FromDto(dto, roundClock);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var viewerId))
        {
            DateTime? submittedAt = null;
            if (viewerId == dto.UserAId)
                submittedAt = dto.UserASubmittedAt;
            else if (viewerId == dto.UserBId)
                submittedAt = dto.UserBSubmittedAt;

            if (submittedAt.HasValue)
                ViewerSubmittedAt = roundClock.ToLocal(submittedAt.Value).ToString("yyyy-MM-dd HH:mm");
        }
    }
}
