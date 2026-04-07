using BldLeague.Application.Commands.LeagueSeasonStandings.Refresh;
using BldLeague.Application.Commands.LeagueSeasonStandings.RefreshAll;
using BldLeague.Application.Commands.LeagueSeasons.Delete;
using BldLeague.Application.Commands.LeagueSeasons.Import;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using BldLeague.Application.Queries.Users.GetByLeagueSeasonId;
using BldLeague.Web.Attributes;
using BldLeague.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.LeagueSeasons;

[AdminOnly]
public class AllLeagueSeasons(IMediator mediator) : PageModel
{
    public IReadOnlyCollection<LeagueSeasonDto> LeagueSeasons { get; private set; } = Array.Empty<LeagueSeasonDto>();
    public ImportResult? ImportResult { get; private set; }

    [BindProperty] public Guid RefreshStandingsLeagueSeasonId { get; set; } = Guid.Empty;
    [BindProperty] public Guid DeleteLeagueSeasonId { get; set; } = Guid.Empty;

    public async Task OnGet()
    {
        LeagueSeasons = await mediator.Send(new GetAllLeagueSeasonsRequest());
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (DeleteLeagueSeasonId == Guid.Empty)
            return RedirectToPage();

        var result = await mediator.Send(new DeleteLeagueSeasonRequest(DeleteLeagueSeasonId));

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRefreshStandingsAsync()
    {
        if (RefreshStandingsLeagueSeasonId == Guid.Empty)
            return RedirectToPage();

        var result = await mediator.Send(new RefreshLeagueSeasonStandingsRequest(RefreshStandingsLeagueSeasonId));

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRefreshAllStandingsAsync()
    {
        var result = await mediator.Send(new RefreshAllLeagueSeasonStandingsRequest());

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportCsv()
    {
        var leagueSeasons = await mediator.Send(new GetAllLeagueSeasonsRequest());
        string[] headers = ["Season Number", "League Identifier"];
        var rows = new List<string?[]>();
        foreach (var ls in leagueSeasons)
        {
            var users = await mediator.Send(new GetUsersByLeagueSeasonIdRequest(ls.LeagueSeasonId));
            var row = new List<string?> { ls.SeasonNumber.ToString(), ls.LeagueIdentifier };
            row.AddRange(users.Select(u => u.WcaId));
            rows.Add(row.ToArray());
        }
        return File(CsvHelper.BuildCsv(headers, rows), "text/csv; charset=utf-8", "league-seasons.csv");
    }

    public async Task<IActionResult> OnPostImportCsvAsync(IFormFile? file)
    {
        LeagueSeasons = await mediator.Send(new GetAllLeagueSeasonsRequest());

        if (file == null || file.Length == 0)
        {
            ImportResult = new ImportResult
            {
                RowResults = [ImportRowResult.Fail(0, "Nie wybrano pliku lub plik jest pusty.")]
            };
            return Page();
        }

        var allRows = await CsvParser.ParseAsync(file);
        var headerRow = allRows.Count > 0 ? allRows[0] : [];
        if (allRows.Count == 0 || headerRow.Length < 2 ||
            headerRow[0] != "Season Number" || headerRow[1] != "League Identifier")
        {
            ImportResult = new ImportResult
            {
                RowResults = [ImportRowResult.Fail(0, "Niepoprawne nagłówki CSV. Oczekiwano: Season Number, League Identifier[, WCA ID, ...]")]
            };
            return Page();
        }

        var rows = new List<ImportLeagueSeasonRow>();
        var parseErrors = new List<ImportRowResult>();

        for (var i = 1; i < allRows.Count; i++)
        {
            var cols = allRows[i];
            if (cols.Length < 2)
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, "Za mało kolumn (oczekiwano co najmniej 2)."));
                continue;
            }
            if (!int.TryParse(cols[0], out var seasonNumber))
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawny numer sezonu: '{cols[0]}'."));
                continue;
            }
            rows.Add(new ImportLeagueSeasonRow
            {
                RowNumber = i + 1,
                SeasonNumber = seasonNumber,
                LeagueIdentifier = cols[1]?.Trim() ?? "",
                WcaIds = cols.Skip(2).Select(c => c?.Trim() ?? "").Where(c => c.Length > 0).ToList()
            });
        }

        if (parseErrors.Count > 0)
        {
            ImportResult = new ImportResult { RowResults = parseErrors };
            return Page();
        }

        ImportResult = await mediator.Send(new ImportLeagueSeasonsRequest { Rows = rows });
        LeagueSeasons = await mediator.Send(new GetAllLeagueSeasonsRequest());
        return Page();
    }

}
