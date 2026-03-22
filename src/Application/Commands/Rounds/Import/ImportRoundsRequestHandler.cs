using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Rounds.Import;

/// <summary>
/// Handles bulk-importing rounds for a season, creating new rounds or updating existing ones and replacing their scrambles.
/// </summary>
public class ImportRoundsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ImportRoundsRequest, ImportResult>
{
    public async Task<ImportResult> Handle(ImportRoundsRequest request, CancellationToken cancellationToken)
    {
        var season = await unitOfWork.SeasonRepository.GetBySeasonNumberAsync(request.SeasonNumber);
        if (season == null)
            return new ImportResult
            {
                RowResults = [ImportRowResult.Fail(0, $"Nie znaleziono sezonu numer: {request.SeasonNumber}")]
            };

        var seasonId = season.Id;
        var existingRounds = await unitOfWork.RoundRepository.GetRoundSummariesBySeasonIdAsync(seasonId);
        var results = new List<ImportRowResult>();

        foreach (var row in request.Rows)
        {
            if (row.RoundNumber <= 0)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Numer kolejki musi być większy od 0 (podano: {row.RoundNumber})."));
                continue;
            }

            if (row.StartDate > row.EndDate)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Kolejka {row.RoundNumber}: data końcowa nie może być wcześniejsza niż początkowa."));
                continue;
            }

            var isUpdate = existingRounds.Any(r => r.RoundNumber == row.RoundNumber);
            results.Add(ImportRowResult.Ok(row.RowNumber,
                isUpdate ? $"Zaktualizowano — Kolejka {row.RoundNumber}" : $"OK — Kolejka {row.RoundNumber}"));
        }

        // Also check for duplicates within the CSV itself
        var rowsByNumber = request.Rows.GroupBy(r => r.RoundNumber).Where(g => g.Count() > 1);
        foreach (var group in rowsByNumber)
        {
            foreach (var row in group)
            {
                var idx = results.FindIndex(r => r.RowNumber == row.RowNumber && r.Success);
                if (idx >= 0)
                    results[idx] = ImportRowResult.Fail(row.RowNumber, $"Kolejka {row.RoundNumber} pojawia się więcej niż raz w pliku CSV.");
            }
        }

        if (results.Any(r => !r.Success))
            return new ImportResult { RowResults = results };

        await unitOfWork.BeginTransactionAsync();

        foreach (var row in request.Rows)
        {
            var existingRoundDto = existingRounds.FirstOrDefault(r => r.RoundNumber == row.RoundNumber);
            Guid roundId;

            if (existingRoundDto != null)
            {
                var round = await unitOfWork.RoundRepository.GetByIdAsync(existingRoundDto.Id);
                round!.StartDate = row.StartDate;
                round!.EndDate = row.EndDate;
                unitOfWork.RoundRepository.Update(round);
                await unitOfWork.ScrambleRepository.DeleteByRoundIdAsync(existingRoundDto.Id);
                roundId = existingRoundDto.Id;
            }
            else
            {
                var round = Round.Create(seasonId, row.RoundNumber, row.StartDate, row.EndDate);
                await unitOfWork.RoundRepository.AddAsync(round);
                roundId = round.Id;
            }

            for (var i = 0; i < row.Scrambles.Length; i++)
            {
                var notation = row.Scrambles[i];
                if (!string.IsNullOrWhiteSpace(notation))
                    await unitOfWork.ScrambleRepository.AddAsync(Scramble.Create(roundId, i + 1, notation));
            }
        }

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        return new ImportResult { RowResults = results };
    }
}
