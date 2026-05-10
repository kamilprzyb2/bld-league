using System.Security.Claims;
using BldLeague.Application.Queries.LeagueSeasons.GetUserLeagueIdForSeason;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Helpers;

public static class PageModelExtensions
{
    public static async Task<Guid?> ResolveCurrentUserLeagueIdAsync(
        this PageModel page,
        IMediator mediator,
        IReadOnlyCollection<Guid> availableLeagueIds,
        Guid seasonId)
    {
        if (page.User.Identity?.IsAuthenticated != true) return null;
        var userIdClaim = page.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return null;
        var userLeagueId = await mediator.Send(new GetUserLeagueIdForSeasonRequest(userId, seasonId));
        return userLeagueId.HasValue && availableLeagueIds.Contains(userLeagueId.Value) ? userLeagueId : null;
    }
}
