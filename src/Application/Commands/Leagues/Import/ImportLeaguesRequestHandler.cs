using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Leagues.Import;

/// <summary>
/// Handles bulk-importing leagues, skipping rows for leagues that already exist.
/// </summary>
public class ImportLeaguesRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ImportLeaguesRequest, ImportResult>
{
    public async Task<ImportResult> Handle(ImportLeaguesRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.LeagueRepository;
        var results = new List<ImportRowResult>();

        foreach (var row in request.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.LeagueIdentifier))
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, "Identyfikator ligi nie może być pusty."));
                continue;
            }
            results.Add(ImportRowResult.Ok(row.RowNumber, $"OK — Liga {row.LeagueIdentifier}"));
        }

        if (results.Any(r => !r.Success))
            return new ImportResult { RowResults = results };

        await unitOfWork.BeginTransactionAsync();

        for (var i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];
            var exists = await repository.ExistsAsync(l => l.LeagueIdentifier == row.LeagueIdentifier);

            if (exists)
            {
                results[i] = ImportRowResult.Ok(row.RowNumber, $"Pominięto — Liga {row.LeagueIdentifier} już istnieje.");
            }
            else
            {
                var league = League.Create(row.LeagueIdentifier);
                await repository.AddAsync(league);
                results[i] = ImportRowResult.Ok(row.RowNumber, $"Dodano — Liga {row.LeagueIdentifier}");
            }
        }

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        return new ImportResult { RowResults = results };
    }
}
