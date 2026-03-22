using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonUsers.Import;

/// <summary>
/// Handles bulk-importing league season user assignments, resolving season/league/user lookups and skipping duplicates.
/// </summary>
public class ImportLeagueSeasonUsersRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ImportLeagueSeasonUsersRequest, ImportResult>
{
    public async Task<ImportResult> Handle(ImportLeagueSeasonUsersRequest request, CancellationToken cancellationToken)
    {
        var results = new List<ImportRowResult>();

        foreach (var row in request.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.WcaId))
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, "WCA ID nie może być puste."));
                continue;
            }
            results.Add(ImportRowResult.Ok(row.RowNumber, $"OK — Sezon {row.SeasonNumber}, Liga {row.LeagueIdentifier}, WCA ID {row.WcaId}"));
        }

        if (results.Any(r => !r.Success))
            return new ImportResult { RowResults = results };

        await unitOfWork.BeginTransactionAsync();

        for (var i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];

            var season = await unitOfWork.SeasonRepository.GetBySeasonNumberAsync(row.SeasonNumber);
            if (season == null)
            {
                results[i] = ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono sezonu numer {row.SeasonNumber}.");
                continue;
            }

            var allLeagues = await unitOfWork.LeagueRepository.GetAllAsync();
            var league = allLeagues.FirstOrDefault(l => l.LeagueIdentifier == row.LeagueIdentifier);
            if (league == null)
            {
                results[i] = ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono ligi '{row.LeagueIdentifier}'.");
                continue;
            }

            var allLeagueSeasons = await unitOfWork.LeagueSeasonRepository.GetAllAsync();
            var leagueSeason = allLeagueSeasons.FirstOrDefault(ls => ls.SeasonId == season.Id && ls.LeagueId == league.Id);
            if (leagueSeason == null)
            {
                results[i] = ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono liga-sezonu dla Sezonu {row.SeasonNumber} i Ligi {row.LeagueIdentifier}.");
                continue;
            }

            var allUsers = await unitOfWork.UserRepository.GetAllAsync();
            var user = allUsers.FirstOrDefault(u => u.WcaId == row.WcaId);
            if (user == null)
            {
                results[i] = ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono użytkownika z WCA ID '{row.WcaId}'.");
                continue;
            }

            var alreadyAssigned = await unitOfWork.LeagueSeasonUserRepository.GetAsync(leagueSeason.Id, user.Id);
            if (alreadyAssigned != null)
            {
                results[i] = ImportRowResult.Ok(row.RowNumber, $"Pominięto — {row.WcaId} jest już w liga-sezonie.");
                continue;
            }

            await unitOfWork.LeagueSeasonUserRepository.AddAsync(LeagueSeasonUser.Create(leagueSeason.Id, user.Id));
            results[i] = ImportRowResult.Ok(row.RowNumber, $"Dodano — {row.WcaId} do liga-sezonu.");
        }

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        return new ImportResult { RowResults = results };
    }
}
