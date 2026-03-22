using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Seasons.Import;

/// <summary>
/// Handles bulk-importing seasons, skipping rows for season numbers that already exist.
/// </summary>
public class ImportSeasonsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ImportSeasonsRequest, ImportResult>
{
    public async Task<ImportResult> Handle(ImportSeasonsRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.SeasonRepository;
        var results = new List<ImportRowResult>();

        foreach (var row in request.Rows)
        {
            if (row.SeasonNumber <= 0)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Numer sezonu musi być większy od 0 (podano: {row.SeasonNumber})."));
                continue;
            }
            results.Add(ImportRowResult.Ok(row.RowNumber, $"OK — Sezon {row.SeasonNumber}"));
        }

        if (results.Any(r => !r.Success))
            return new ImportResult { RowResults = results };

        await unitOfWork.BeginTransactionAsync();

        for (var i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];
            var exists = await repository.ExistsAsync(s => s.SeasonNumber == row.SeasonNumber);

            if (exists)
            {
                results[i] = ImportRowResult.Ok(row.RowNumber, $"Pominięto — Sezon {row.SeasonNumber} już istnieje.");
            }
            else
            {
                var season = Season.Create(row.SeasonNumber);
                await repository.AddAsync(season);
                results[i] = ImportRowResult.Ok(row.RowNumber, $"Dodano — Sezon {row.SeasonNumber}");
            }
        }

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        return new ImportResult { RowResults = results };
    }
}
