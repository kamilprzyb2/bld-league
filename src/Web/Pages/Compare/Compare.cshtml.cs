using BldLeague.Application.Queries.Users.GetAll;
using BldLeague.Application.Queries.Users.GetHeadToHeadComparison;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Compare;

public class Compare(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid UserAId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid UserBId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? PlayerA { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Opponent { get; set; }

    public HeadToHeadComparisonDto? Comparison { get; set; }
    public IReadOnlyCollection<UserSummaryDto> AllUsers { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGet()
    {
        // Always needed: either for the picker form or for the inline player-change datalist.
        AllUsers = await mediator.Send(new GetAllUsersRequest());

        if (UserAId == Guid.Empty && !string.IsNullOrWhiteSpace(PlayerA))
            UserAId = ResolveUserId(AllUsers, PlayerA!);

        if (UserBId == Guid.Empty && !string.IsNullOrWhiteSpace(Opponent))
            UserBId = ResolveUserId(AllUsers, Opponent!);

        if (UserAId == Guid.Empty || UserBId == Guid.Empty)
            return Page();

        if (UserAId == UserBId)
            return RedirectToPage("/Users/UserProfile", new { id = UserAId });

        Comparison = await mediator.Send(new GetHeadToHeadComparisonRequest(UserAId, UserBId));
        if (Comparison == null)
            return NotFound();

        return Page();
    }

    private Guid ResolveUserId(IReadOnlyCollection<UserSummaryDto> users, string input)
    {
        var trimmed = input.Trim();

        var matched = users
            .Where(u => string.Equals($"{u.FullName} ({u.WcaId})", trimmed, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matched.Count == 0)
            matched = users
                .Where(u => string.Equals(u.FullName, trimmed, StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (matched.Count == 1)
            return matched[0].Id;

        ErrorMessage = "Nie znaleziono zawodnika o podanej nazwie.";
        return Guid.Empty;
    }
}
