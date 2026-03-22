using BldLeague.Application.Commands.Matches.Delete;
using BldLeague.Application.Commands.Matches.Import;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Matches.GetAll;
using BldLeague.Application.Queries.Matches.GetMatchExport;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using BldLeague.Web.Attributes;
using BldLeague.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Matches;

[AdminOnly]
public class AllMatches(IMediator mediator) : PageModel
{
    private static readonly string[] ExpectedHeaders =
    [
        "Season", "League", "Round",
        "WCA ID A", "WCA ID B",
        "Solve A1", "Solve A2", "Solve A3", "Solve A4", "Solve A5",
        "Solve B1", "Solve B2", "Solve B3", "Solve B4", "Solve B5"
    ];

    public IReadOnlyCollection<MatchAdminSummaryDto> Matches { get; private set; } = Array.Empty<MatchAdminSummaryDto>();
    public ImportResult? ImportResult { get; private set; }

    [BindProperty(SupportsGet = true)] public int? SeasonNumber { get; set; }
    [BindProperty(SupportsGet = true)] public string? LeagueIdentifier { get; set; }
    [BindProperty(SupportsGet = true)] public int? RoundNumber { get; set; }
    [BindProperty] public Guid RemoveMatchId { get; set; } = Guid.Empty;

    public async Task OnGet()
    {
        Matches = await mediator.Send(new GetAllMatchesRequest());
    }

    public async Task<IActionResult> OnPostRemoveMatchAsync()
    {
        if (RemoveMatchId == Guid.Empty)
            return RedirectToPage();

        var result = await mediator.Send(new DeleteMatchRequest(RemoveMatchId));

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportCsv()
    {
        var matches = await mediator.Send(new GetMatchExportRequest
        {
            SeasonNumber = SeasonNumber,
            LeagueIdentifier = LeagueIdentifier,
            RoundNumber = RoundNumber
        });

        string[] headers =
        [
            "Season", "League", "Round",
            "WCA ID A", "WCA ID B",
            "Solve A1", "Solve A2", "Solve A3", "Solve A4", "Solve A5",
            "Solve B1", "Solve B2", "Solve B3", "Solve B4", "Solve B5"
        ];

        var rows = matches.Select(m =>
        {
            var fields = new string?[5 + Match.SOLVES_PER_MATCH * 2];
            fields[0] = m.SeasonNumber.ToString();
            fields[1] = m.LeagueIdentifier;
            fields[2] = m.RoundNumber.ToString();
            fields[3] = m.UserAWcaId;
            fields[4] = m.UserBWcaId;
            for (var i = 0; i < Match.SOLVES_PER_MATCH; i++)
                fields[5 + i] = m.SolvesUserA.ElementAtOrDefault(i).ToString();
            for (var i = 0; i < Match.SOLVES_PER_MATCH; i++)
                fields[5 + Match.SOLVES_PER_MATCH + i] = m.SolvesUserB.Count > 0
                    ? m.SolvesUserB.ElementAtOrDefault(i).ToString()
                    : null;
            return fields;
        });

        return File(CsvHelper.BuildCsv(headers, rows), "text/csv; charset=utf-8", "matches.csv");
    }

    public async Task OnPostImportCsv(IFormFile? file)
    {
        Matches = await mediator.Send(new GetAllMatchesRequest());

        if (file == null || file.Length == 0)
        {
            ImportResult = new ImportResult
            {
                RowResults = [ImportRowResult.Fail(0, "Nie wybrano pliku lub plik jest pusty.")]
            };
            return;
        }

        var allRows = await CsvParser.ParseAsync(file);
        if (allRows.Count == 0 || !ExpectedHeaders.SequenceEqual(allRows[0].Select(h => h ?? "")))
        {
            ImportResult = new ImportResult
            {
                RowResults = [ImportRowResult.Fail(0, $"Niepoprawne nagłówki CSV. Oczekiwano: {string.Join(", ", ExpectedHeaders)}")]
            };
            return;
        }

        var rows = new List<ImportMatchRow>();
        var parseErrors = new List<ImportRowResult>();
        const int totalCols = 5 + Match.SOLVES_PER_MATCH * 2;

        for (var i = 1; i < allRows.Count; i++)
        {
            var cols = allRows[i];
            if (cols.Length < totalCols)
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Za mało kolumn (oczekiwano {totalCols}, znaleziono {cols.Length})."));
                continue;
            }

            if (!int.TryParse(cols[0], out var seasonNumber))
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawny numer sezonu: '{cols[0]}'."));
                continue;
            }

            if (!int.TryParse(cols[2], out var roundNumber))
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawny numer kolejki: '{cols[2]}'."));
                continue;
            }

            var solvesA = new List<SolveResult>();
            var solvesB = new List<SolveResult>();
            var solveError = false;

            for (var s = 0; s < Match.SOLVES_PER_MATCH; s++)
            {
                try { solvesA.Add(SolveResult.FromString(cols[5 + s])); }
                catch
                {
                    parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawny wynik Solve A{s + 1}: '{cols[5 + s]}'."));
                    solveError = true;
                    break;
                }
            }
            if (solveError) continue;

            for (var s = 0; s < Match.SOLVES_PER_MATCH; s++)
            {
                try { solvesB.Add(SolveResult.FromString(cols[5 + Match.SOLVES_PER_MATCH + s])); }
                catch
                {
                    parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawny wynik Solve B{s + 1}: '{cols[5 + Match.SOLVES_PER_MATCH + s]}'."));
                    solveError = true;
                    break;
                }
            }
            if (solveError) continue;

            rows.Add(new ImportMatchRow
            {
                RowNumber = i + 1,
                SeasonNumber = seasonNumber,
                LeagueIdentifier = cols[1]?.Trim() ?? "",
                RoundNumber = roundNumber,
                WcaIdA = cols[3]?.Trim() ?? "",
                WcaIdB = string.IsNullOrWhiteSpace(cols[4]) ? null : cols[4]!.Trim(),
                SolvesA = solvesA,
                SolvesB = solvesB,
            });
        }

        if (parseErrors.Count > 0)
        {
            ImportResult = new ImportResult { RowResults = parseErrors };
            return;
        }

        ImportResult = await mediator.Send(new ImportMatchesRequest { Rows = rows });
        Matches = await mediator.Send(new GetAllMatchesRequest());
    }
}
