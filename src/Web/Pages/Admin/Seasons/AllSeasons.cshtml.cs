using BldLeague.Application.Commands.Seasons.Delete;
using BldLeague.Application.Commands.Seasons.Import;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Web.Attributes;
using BldLeague.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Seasons;

[AdminOnly]
public class AllSeasons(IMediator mediator) : PageModel
{
    private static readonly string[] ExpectedHeaders = ["Season Number"];

    public IReadOnlyCollection<SeasonSummaryDto> Seasons { get; private set; } = Array.Empty<SeasonSummaryDto>();
    public ImportResult? ImportResult { get; private set; }

    [BindProperty] public Guid DeleteSeasonId { get; set; } = Guid.Empty;

    public async Task OnGet()
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (DeleteSeasonId == Guid.Empty)
            return RedirectToPage();

        var result = await mediator.Send(new DeleteSeasonRequest(DeleteSeasonId));

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportCsv()
    {
        var seasons = await mediator.Send(new GetAllSeasonsRequest());
        string[] headers = ["Season Number"];
        var rows = seasons.Select(s => new string?[] { s.SeasonNumber.ToString() });
        return File(CsvHelper.BuildCsv(headers, rows), "text/csv; charset=utf-8", "seasons.csv");
    }

    public async Task<IActionResult> OnPostImportCsvAsync(IFormFile? file)
    {
        Seasons = await mediator.Send(new GetAllSeasonsRequest());

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

        var rows = new List<ImportSeasonRow>();
        var parseErrors = new List<ImportRowResult>();

        for (var i = 1; i < allRows.Count; i++)
        {
            var cols = allRows[i];
            if (cols.Length < 1 || !int.TryParse(cols[0], out var seasonNumber))
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawny numer sezonu: '{cols.ElementAtOrDefault(0)}'."));
                continue;
            }
            rows.Add(new ImportSeasonRow { RowNumber = i + 1, SeasonNumber = seasonNumber });
        }

        if (parseErrors.Count > 0)
        {
            ImportResult = new ImportResult { RowResults = parseErrors };
            return Page();
        }

        ImportResult = await mediator.Send(new ImportSeasonsRequest { Rows = rows });
        Seasons = await mediator.Send(new GetAllSeasonsRequest());
        return Page();
    }
}
