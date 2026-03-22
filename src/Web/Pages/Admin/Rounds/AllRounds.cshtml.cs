using BldLeague.Application.Commands.RoundStandings.Refresh;
using BldLeague.Application.Commands.Rounds.Delete;
using BldLeague.Application.Commands.Rounds.Import;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Rounds.GetAll;
using BldLeague.Application.Queries.Rounds.GetScrambles;
using BldLeague.Domain.Entities;
using BldLeague.Web.Attributes;
using BldLeague.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Rounds;

[AdminOnly]
public class AllRounds(IMediator mediator) : PageModel
{
    private static readonly string[] RequiredHeaders = ["Season", "Round", "Start Date", "End Date"];
    private static readonly string[] ScrambleHeaders =
        Enumerable.Range(1, Match.SOLVES_PER_MATCH).Select(i => $"Scramble {i}").ToArray();

    public IReadOnlyCollection<RoundAdminSummaryDto> Rounds { get; private set; } = Array.Empty<RoundAdminSummaryDto>();
    public ImportResult? ImportResult { get; private set; }
    public Dictionary<Guid, IReadOnlyCollection<ScrambleDto>> RoundScrambles { get; private set; } = new();

    [BindProperty] public Guid RemoveRoundId { get; set; } = Guid.Empty;
    [BindProperty] public Guid RefreshStandingsRoundId { get; set; } = Guid.Empty;

    public async Task OnGet()
    {
        Rounds = await mediator.Send(new GetAllRoundsRequest());
        await LoadScrambles();
    }

    public async Task<IActionResult> OnPostRemoveRoundAsync()
    {
        if (RemoveRoundId == Guid.Empty)
            return RedirectToPage();

        var result = await mediator.Send(new DeleteRoundRequest(RemoveRoundId));

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRefreshStandingsAsync()
    {
        if (RefreshStandingsRoundId == Guid.Empty)
            return RedirectToPage();

        var result = await mediator.Send(new RefreshRoundStandingsRequest(RefreshStandingsRoundId));

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportCsv()
    {
        var rounds = await mediator.Send(new GetAllRoundsRequest());
        var scramblesByRound = new Dictionary<Guid, IReadOnlyCollection<ScrambleDto>>();
        foreach (var round in rounds)
            scramblesByRound[round.Id] = await mediator.Send(new GetScramblesByRoundIdRequest(round.Id));

        string[] headers = [..RequiredHeaders, ..ScrambleHeaders];
        var rows = rounds.Select(r =>
        {
            var scrambles = scramblesByRound.TryGetValue(r.Id, out var s) ? s : Array.Empty<ScrambleDto>();
            var notations = Enumerable.Range(1, Match.SOLVES_PER_MATCH)
                .Select(i => scrambles.FirstOrDefault(x => x.ScrambleNumber == i)?.Notation)
                .ToArray();
            return new string?[]
            {
                r.SeasonNumber.ToString(),
                r.RoundNumber.ToString(),
                r.StartDate.ToString("dd.MM.yyyy"),
                r.EndDate.ToString("dd.MM.yyyy"),
            }.Concat(notations).ToArray();
        });

        return File(CsvHelper.BuildCsv(headers, rows), "text/csv; charset=utf-8", "rounds.csv");
    }

    public async Task<IActionResult> OnPostImportCsvAsync(IFormFile? file)
    {
        Rounds = await mediator.Send(new GetAllRoundsRequest());

        if (file == null || file.Length == 0)
        {
            ImportResult = new ImportResult
            {
                RowResults = [ImportRowResult.Fail(0, "Nie wybrano pliku lub plik jest pusty.")]
            };
            await LoadScrambles();
            return Page();
        }

        var allRows = await CsvParser.ParseAsync(file);
        var headers = allRows.Count > 0 ? allRows[0].Select(h => h ?? "").ToArray() : [];
        if (headers.Length < RequiredHeaders.Length || !RequiredHeaders.SequenceEqual(headers.Take(RequiredHeaders.Length)))
        {
            ImportResult = new ImportResult
            {
                RowResults = [ImportRowResult.Fail(0, $"Niepoprawne nagłówki CSV. Oczekiwano: {string.Join(", ", RequiredHeaders)}[, {string.Join(", ", ScrambleHeaders)}]")]
            };
            await LoadScrambles();
            return Page();
        }

        var rowsBySeasonNumber = new Dictionary<int, List<ImportRoundRow>>();
        var parseErrors = new List<ImportRowResult>();

        for (var i = 1; i < allRows.Count; i++)
        {
            var cols = allRows[i];
            if (cols.Length < RequiredHeaders.Length)
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Za mało kolumn (oczekiwano co najmniej {RequiredHeaders.Length}, znaleziono {cols.Length})."));
                continue;
            }

            if (!int.TryParse(cols[0], out var seasonNumber))
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawny numer sezonu: '{cols[0]}'."));
                continue;
            }

            if (!int.TryParse(cols[1], out var roundNumber))
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawny numer kolejki: '{cols[1]}'."));
                continue;
            }

            if (!DateTime.TryParseExact(cols[2], "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var startDate))
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawna data rozpoczęcia: '{cols[2]}' (oczekiwano dd.MM.yyyy)."));
                continue;
            }

            if (!DateTime.TryParseExact(cols[3], "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var endDate))
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Niepoprawna data zakończenia: '{cols[3]}' (oczekiwano dd.MM.yyyy)."));
                continue;
            }

            var scrambles = new string?[Match.SOLVES_PER_MATCH];
            for (var j = 0; j < Match.SOLVES_PER_MATCH; j++)
            {
                var colIndex = RequiredHeaders.Length + j;
                scrambles[j] = colIndex < cols.Length ? cols[colIndex] : null;
            }

            if (!rowsBySeasonNumber.ContainsKey(seasonNumber))
                rowsBySeasonNumber[seasonNumber] = [];

            rowsBySeasonNumber[seasonNumber].Add(new ImportRoundRow
            {
                RowNumber = i + 1,
                RoundNumber = roundNumber,
                StartDate = startDate.ToUniversalTime(),
                EndDate = endDate.ToUniversalTime(),
                Scrambles = scrambles
            });
        }

        if (parseErrors.Count > 0)
        {
            ImportResult = new ImportResult { RowResults = parseErrors };
            await LoadScrambles();
            return Page();
        }

        var allResults = new List<ImportRowResult>();
        foreach (var (seasonNumber, rows) in rowsBySeasonNumber)
        {
            var result = await mediator.Send(new ImportRoundsRequest { SeasonNumber = seasonNumber, Rows = rows });
            allResults.AddRange(result.RowResults);
        }

        ImportResult = new ImportResult { RowResults = allResults };
        Rounds = await mediator.Send(new GetAllRoundsRequest());
        await LoadScrambles();
        return Page();
    }

    private async Task LoadScrambles()
    {
        foreach (var round in Rounds)
            RoundScrambles[round.Id] = await mediator.Send(new GetScramblesByRoundIdRequest(round.Id));
    }
}
