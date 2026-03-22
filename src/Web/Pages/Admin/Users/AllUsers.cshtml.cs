using System.Text.RegularExpressions;
using BldLeague.Application.Commands.Users.Delete;
using BldLeague.Application.Commands.Users.Import;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Users.GetAll;
using BldLeague.Web.Attributes;
using BldLeague.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Admin.Users;

[AdminOnly]
public class AllUsers(IMediator mediator) : PageModel
{
    private static readonly string[] ExpectedHeaders =
        ["Full Name", "WCA ID", "Admin", "Avatar URL", "Avatar Thumbnail URL"];

    public IReadOnlyCollection<UserSummaryDto> Users { get; private set; } = Array.Empty<UserSummaryDto>();
    public ImportResult? ImportResult { get; private set; }

    [BindProperty] public Guid DeleteUserId { get; set; } = Guid.Empty;

    public async Task OnGet()
    {
        Users = await mediator.Send(new GetAllUsersRequest());
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (DeleteUserId == Guid.Empty)
            return RedirectToPage();

        var result = await mediator.Send(new DeleteUserRequest(DeleteUserId));

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportCsv()
    {
        var users = await mediator.Send(new GetAllUsersRequest());
        string[] headers = ["Full Name", "WCA ID", "Admin", "Avatar URL", "Avatar Thumbnail URL"];
        var rows = users.Select(u => new string?[]
        {
            u.FullName, u.WcaId, u.IsAdmin ? "Yes" : "No", u.AvatarUrl, u.AvatarThumbnailUrl
        });
        return File(CsvHelper.BuildCsv(headers, rows), "text/csv; charset=utf-8", "users.csv");
    }

    public async Task OnPostImportCsv(IFormFile? file)
    {
        Users = await mediator.Send(new GetAllUsersRequest());

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

        var rows = new List<ImportUserRow>();
        var parseErrors = new List<ImportRowResult>();

        for (var i = 1; i < allRows.Count; i++)
        {
            var cols = allRows[i];
            if (cols.Length < 5)
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Za mało kolumn (oczekiwano 5, znaleziono {cols.Length})."));
                continue;
            }

            var adminRaw = cols[2]?.Trim();
            if (adminRaw != "Yes" && adminRaw != "No")
            {
                parseErrors.Add(ImportRowResult.Fail(i + 1, $"Pole 'Admin' musi być 'Yes' lub 'No' (podano: '{adminRaw}')."));
                continue;
            }

            rows.Add(new ImportUserRow
            {
                RowNumber = i + 1,
                FullName = cols[0]?.Trim() ?? "",
                WcaId = cols[1]?.Trim() ?? "",
                IsAdmin = adminRaw == "Yes",
                AvatarUrl = cols[3],
                AvatarThumbnailUrl = cols[4],
            });
        }

        if (parseErrors.Count > 0)
        {
            ImportResult = new ImportResult { RowResults = parseErrors };
            return;
        }

        ImportResult = await mediator.Send(new ImportUsersRequest { Rows = rows });
    }
}
