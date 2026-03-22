using System.Text.RegularExpressions;
using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Users.Import;

/// <summary>
/// Handles bulk-importing users, creating new users or updating existing ones matched by WCA ID within a single transaction.
/// </summary>
public class ImportUsersRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ImportUsersRequest, ImportResult>
{
    private static readonly Regex WcaIdRegex = new(@"^\d{4}[A-Z]{4}\d{2}$", RegexOptions.Compiled);

    public async Task<ImportResult> Handle(ImportUsersRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.UserRepository;
        var results = new List<ImportRowResult>();

        // Validate all rows first — abort on any error
        foreach (var row in request.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.FullName))
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, "Pole 'Full Name' jest wymagane."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.WcaId) || !WcaIdRegex.IsMatch(row.WcaId))
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"WCA ID '{row.WcaId}' ma niepoprawny format."));
                continue;
            }

            results.Add(ImportRowResult.Ok(row.RowNumber, $"OK — {row.FullName} ({row.WcaId})"));
        }

        if (results.Any(r => !r.Success))
            return new ImportResult { RowResults = results };

        // All rows valid — write in a single transaction
        await unitOfWork.BeginTransactionAsync();

        for (var i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];
            var existing = await repository.GetUserDetailByWcaIdAsync(row.WcaId);

            if (existing != null)
            {
                var updated = new User
                {
                    Id = existing.Id,
                    FullName = row.FullName,
                    WcaId = row.WcaId,
                    AvatarUrl = row.AvatarUrl,
                    AvatarThumbnailUrl = row.AvatarThumbnailUrl,
                    IsAdmin = row.IsAdmin,
                };
                repository.Update(updated);
                results[i] = ImportRowResult.Ok(row.RowNumber, $"Zaktualizowano — {row.FullName} ({row.WcaId})");
            }
            else
            {
                var user = User.Create(row.FullName, row.WcaId, row.AvatarUrl, row.AvatarThumbnailUrl, row.IsAdmin);
                await repository.AddAsync(user);
                results[i] = ImportRowResult.Ok(row.RowNumber, $"Dodano — {row.FullName} ({row.WcaId})");
            }
        }

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        return new ImportResult { RowResults = results };
    }
}
