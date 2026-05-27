using BldLeague.Application.Queries.Matches.GetRecentFinishedMatches;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Matches;

public class Recent(IMediator mediator) : PageModel
{
    private const int RecentMatchCount = 21;

    public IReadOnlyList<RecentMatchDto> RecentMatches { get; set; } = [];

    public async Task OnGet()
    {
        RecentMatches = await mediator.Send(new GetRecentFinishedMatchesRequest(RecentMatchCount));
    }
}
