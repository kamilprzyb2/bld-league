using BldLeague.Application.Commands.Leagues.Delete;
using BldLeague.Application.Commands.Leagues.Import;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Leagues.GetAll;
using BldLeague.Web.Attributes;
using BldLeague.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Leagues;

[AdminOnly]
public class AllLeagues(IMediator mediator) : PageModel
{
    private static readonly string[] ExpectedHeaders = ["League Identifier"];

    public IReadOnlyCollection<LeagueSummaryDto> Leagues { get; private set; } = Array.Empty<LeagueSummaryDto>();
    public ImportResult? ImportResult { get; private set; }

    [BindProperty] public Guid DeleteLeagueId { get; set; } = Guid.Empty;

    public async Task OnGet()
    {
        Leagues = await mediator.Send(new GetAllLeaguesRequest());
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (DeleteLeagueId == Guid.Empty)
            return RedirectToPage();

        var result = await mediator.Send(new DeleteLeagueRequest(DeleteLeagueId));

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportCsv()
    {
        var leagues = await mediator.Send(new GetAllLeaguesRequest());
        string[] headers = ["League Identifier"];
        var rows = leagues.Select(l => new string?[] { l.LeagueIdentifier });
        return File(CsvHelper.BuildCsv(headers, rows), "text/csv; charset=utf-8", "leagues.csv");
    }

    public async Task<IActionResult> OnPostImportCsvAsync(IFormFile? file)
    {
        Leagues = await mediator.Send(new GetAllLeaguesRequest());

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

        var rows = new List<ImportLeagueRow>();
        var parseErrors = new List<ImportRowResult>();

        for (var i = 1; i < allRows.Count; i++)
        {
            var cols = allRows[i];
            if (cols.Length < 1)
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, "Za mało kolumn (oczekiwano 1)."));
                continue;
            }
            rows.Add(new ImportLeagueRow { RowNumber = i + 1, LeagueIdentifier = cols[0]?.Trim() ?? "" });
        }

        if (parseErrors.Count > 0)
        {
            ImportResult = new ImportResult { RowResults = parseErrors };
            return Page();
        }

        ImportResult = await mediator.Send(new ImportLeaguesRequest { Rows = rows });
        Leagues = await mediator.Send(new GetAllLeaguesRequest());
        return Page();
    }
}
