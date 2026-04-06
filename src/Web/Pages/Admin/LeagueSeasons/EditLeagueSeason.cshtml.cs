using BldLeague.Application.Commands.LeagueSeasonUsers.Create;
using BldLeague.Application.Commands.LeagueSeasonUsers.Delete;
using BldLeague.Application.Commands.LeagueSeasonUsers.Import;
using BldLeague.Application.Commands.LeagueSeasonUsers.SetGroup;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using BldLeague.Application.Queries.LeagueSeasons.GetById;
using BldLeague.Application.Queries.Users.GetAll;
using BldLeague.Application.Queries.Users.GetByLeagueSeasonId;
using BldLeague.Application.Queries.Users.GetUnassigned;
using BldLeague.Web.Attributes;
using BldLeague.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.LeagueSeasons;

[AdminOnly]
public class EditLeagueSeason(IMediator mediator) : PageModel
{
    private static readonly string[] ExpectedHeaders = ["Season Number", "League Identifier", "WCA ID"];

    [FromRoute]
    public Guid Id { get; set; }

    public LeagueSeasonDto? LeagueSeason { get; set; }
    public IReadOnlyCollection<LeagueSeasonUserDto> AssignedUsers { get; set; } = Array.Empty<LeagueSeasonUserDto>();
    public IReadOnlyCollection<UserSummaryDto> UnassignedUsers { get; set; } = Array.Empty<UserSummaryDto>();
    public ImportResult? ImportResult { get; private set; }

    [BindProperty] public List<Guid> SelectedUserIds { get; set; } = [];
    [BindProperty] public Guid? RemoveUserId { get; set; }
    [BindProperty] public Guid? SetGroupUserId { get; set; }
    [BindProperty] public int SetGroupValue { get; set; }

    public async Task<IActionResult> OnGet()
    {
        LeagueSeason = await mediator.Send(new GetLeagueSeasonByIdRequest(Id));
        if (LeagueSeason == null)
        {
            TempData["ErrorMessage"] = $"Nie znaleziono sezonu w lidze z id: {Id}.";
            return RedirectToPage("/Admin/LeagueSeasons/AllLeagueSeasons");
        }

        AssignedUsers = await mediator.Send(new GetUsersByLeagueSeasonIdRequest(Id));
        UnassignedUsers = await mediator.Send(new GetUnassignedUsersBySeasonIdRequest(LeagueSeason.SeasonId));

        return Page();
    }

    public async Task<IActionResult> OnPostAddUsersAsync()
    {
        if (SelectedUserIds.Count == 0)
            return RedirectToPage();

        var request = new CreateLeagueSeasonUsersRequest
        {
            LeagueSeasonId = Id,
            UserIds = SelectedUserIds
        };
        var result = await mediator.Send(request);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveUserAsync()
    {
        if (!RemoveUserId.HasValue)
            return RedirectToPage();

        var request = new DeleteLeagueSeasonUserRequest
        {
            UserId = RemoveUserId.Value,
            LeagueSeasonId = Id
        };

        var result = await mediator.Send(request);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetGroupAsync()
    {
        if (!SetGroupUserId.HasValue)
            return RedirectToPage();

        var request = new SetLeagueSeasonUserGroupRequest
        {
            LeagueSeasonId = Id,
            UserId = SetGroupUserId.Value,
            SubleagueGroup = SetGroupValue
        };

        var result = await mediator.Send(request);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportUsersCsv()
    {
        var leagueSeason = await mediator.Send(new GetLeagueSeasonByIdRequest(Id));
        if (leagueSeason == null)
            return NotFound();

        var users = await mediator.Send(new GetUsersByLeagueSeasonIdRequest(Id));
        string[] headers = ["Season Number", "League Identifier", "WCA ID"];
        var rows = users.Select(u => new string?[]
        {
            leagueSeason.SeasonNumber.ToString(),
            leagueSeason.LeagueIdentifier,
            u.WcaId
        });
        return File(CsvHelper.BuildCsv(headers, rows), "text/csv; charset=utf-8", $"league-season-users-s{leagueSeason.SeasonNumber}-{leagueSeason.LeagueIdentifier}.csv");
    }

    public async Task<IActionResult> OnPostImportUsersCsvAsync(IFormFile? file)
    {
        LeagueSeason = await mediator.Send(new GetLeagueSeasonByIdRequest(Id));
        if (LeagueSeason == null)
        {
            TempData["ErrorMessage"] = $"Nie znaleziono sezonu w lidze z id: {Id}.";
            return RedirectToPage("/Admin/LeagueSeasons/AllLeagueSeasons");
        }

        AssignedUsers = await mediator.Send(new GetUsersByLeagueSeasonIdRequest(Id));
        UnassignedUsers = await mediator.Send(new GetUnassignedUsersBySeasonIdRequest(LeagueSeason.SeasonId));

        if (file == null || file.Length == 0)
        {
            ImportResult = new ImportResult
            {
                RowResults = [ImportRowResult.Fail(0, "Nie wybrano pliku lub plik jest pusty.")]
            };
            return Page();
        }

        var allRows = await CsvParser.ParseAsync(file);
        if (allRows.Count == 0 || !ExpectedHeaders.SequenceEqual(allRows[0].Select(h => h ?? "")))
        {
            ImportResult = new ImportResult
            {
                RowResults = [ImportRowResult.Fail(0, $"Niepoprawne nagłówki CSV. Oczekiwano: {string.Join(", ", ExpectedHeaders)}")]
            };
            return Page();
        }

        var rows = new List<ImportLeagueSeasonUserRow>();
        var parseErrors = new List<ImportRowResult>();

        for (var i = 1; i < allRows.Count; i++)
        {
            var cols = allRows[i];
            if (cols.Length < 3)
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, "Za mało kolumn (oczekiwano 3)."));
                continue;
            }
            if (!int.TryParse(cols[0], out var seasonNumber))
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawny numer sezonu: '{cols[0]}'."));
                continue;
            }
            rows.Add(new ImportLeagueSeasonUserRow
            {
                RowNumber = i + 1,
                SeasonNumber = seasonNumber,
                LeagueIdentifier = cols[1]?.Trim() ?? "",
                WcaId = cols[2]?.Trim() ?? ""
            });
        }

        if (parseErrors.Count > 0)
        {
            ImportResult = new ImportResult { RowResults = parseErrors };
            return Page();
        }

        ImportResult = await mediator.Send(new ImportLeagueSeasonUsersRequest { Rows = rows });
        AssignedUsers = await mediator.Send(new GetUsersByLeagueSeasonIdRequest(Id));
        UnassignedUsers = await mediator.Send(new GetUnassignedUsersBySeasonIdRequest(LeagueSeason.SeasonId));
        return Page();
    }
}
