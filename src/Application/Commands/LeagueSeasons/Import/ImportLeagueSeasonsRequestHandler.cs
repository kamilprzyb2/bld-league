using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasons.Import;

/// <summary>
/// Handles bulk-importing league seasons and their user rosters, creating or reusing league seasons and skipping already-assigned users.
/// </summary>
public class ImportLeagueSeasonsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ImportLeagueSeasonsRequest, ImportResult>
{
    public async Task<ImportResult> Handle(ImportLeagueSeasonsRequest request, CancellationToken cancellationToken)
    {
        var allSeasons = await unitOfWork.SeasonRepository.GetAllAsync();
        var allLeagues = await unitOfWork.LeagueRepository.GetAllAsync();

        var results = new List<ImportRowResult>();
        // Resolved data index-matched with request.Rows
        var resolved = new List<(Season season, League league, List<(string wcaId, Guid userId)> users)?>();

        // Validation phase
        foreach (var row in request.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.LeagueIdentifier))
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, "Identyfikator ligi nie może być pusty."));
                resolved.Add(null);
                continue;
            }

            var season = allSeasons.FirstOrDefault(s => s.SeasonNumber == row.SeasonNumber);
            if (season == null)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono sezonu numer {row.SeasonNumber}."));
                resolved.Add(null);
                continue;
            }

            var league = allLeagues.FirstOrDefault(l => l.LeagueIdentifier == row.LeagueIdentifier);
            if (league == null)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono ligi '{row.LeagueIdentifier}'."));
                resolved.Add(null);
                continue;
            }

            var userIds = new List<(string wcaId, Guid userId)>();
            bool valid = true;
            foreach (var wcaId in row.WcaIds)
            {
                var user = await unitOfWork.UserRepository.GetUserDetailByWcaIdAsync(wcaId);
                if (user == null)
                {
                    results.Add(ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono użytkownika z WCA ID '{wcaId}'."));
                    valid = false;
                    break;
                }
                userIds.Add((wcaId, user.Id));
            }

            if (!valid) { resolved.Add(null); continue; }

            results.Add(ImportRowResult.Ok(row.RowNumber, $"OK — Sezon {row.SeasonNumber}, Liga {row.LeagueIdentifier}"));
            resolved.Add((season, league, userIds));
        }

        if (results.Any(r => !r.Success))
            return new ImportResult { RowResults = results };

        await unitOfWork.BeginTransactionAsync();

        for (var i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];
            var (season, league, userIds) = resolved[i]!.Value;

            // Find or create league season
            Guid leagueSeasonId;
            string action;

            var exists = await unitOfWork.LeagueSeasonRepository.ExistsAsync(
                ls => ls.SeasonId == season.Id && ls.LeagueId == league.Id);

            if (!exists)
            {
                var newLs = LeagueSeason.Create(league, season);
                await unitOfWork.LeagueSeasonRepository.AddAsync(newLs);
                leagueSeasonId = newLs.Id;
                action = "Dodano";
            }
            else
            {
                var leagueSeasons = await unitOfWork.LeagueSeasonRepository.GetLeagueSeasonsForSeasonIdAsync(season.Id);
                leagueSeasonId = leagueSeasons.First(ls => ls.LeagueId == league.Id).LeagueSeasonId;
                action = "Pominięto (liga-sezon istnieje)";
            }

            // Add users not already assigned
            int added = 0;
            foreach (var (_, userId) in userIds)
            {
                var alreadyIn = await unitOfWork.LeagueSeasonUserRepository.GetAsync(leagueSeasonId, userId);
                if (alreadyIn == null)
                {
                    await unitOfWork.LeagueSeasonUserRepository.AddAsync(LeagueSeasonUser.Create(leagueSeasonId, userId));
                    added++;
                }
            }

            var userSuffix = userIds.Count > 0 ? $", dodano {added}/{userIds.Count} uczestników" : "";
            results[i] = ImportRowResult.Ok(row.RowNumber,
                $"{action} — Sezon {row.SeasonNumber}, Liga {row.LeagueIdentifier}{userSuffix}");
        }

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        return new ImportResult { RowResults = results };
    }
}
