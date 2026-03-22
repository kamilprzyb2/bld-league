using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Web.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Matches;

public class ViewMatch(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    
    public MatchDetailsViewModel? MatchDetails { get; set; }
    
    public async Task OnGet()
    {
        var dto = await mediator.Send(new GetMatchDetailsByIdRequest(Id));
        MatchDetails = dto == null ? null : MatchDetailsViewModel.FromDto(dto);
    }
}