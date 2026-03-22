using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Commands.RoundStandings.Refresh;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Import;

/// <summary>
/// Handles bulk-importing matches, replacing existing matches for the same player/round, and refreshing standings for all affected rounds.
/// </summary>
public class ImportMatchesRequestHandler(IUnitOfWork unitOfWork, ISender sender)
    : IRequestHandler<ImportMatchesRequest, ImportResult>
{
    public async Task<ImportResult> Handle(ImportMatchesRequest request, CancellationToken cancellationToken)
    {
        // Pre-load seasons and leagues once for lookups
        var allSeasons = await unitOfWork.SeasonRepository.GetAllAsync();
        var allLeagues = await unitOfWork.LeagueRepository.GetAllAsync();

        // Caches keyed by seasonId to avoid repeated DB calls
        var leagueSeasonCache = new Dictionary<Guid, IReadOnlyCollection<LeagueSeasonDto>>();
        var roundCache = new Dictionary<Guid, IReadOnlyCollection<RoundSummaryDto>>();

        var results = new List<ImportRowResult>();
        // Tracks (leagueSeasonId, roundId, userId) assigned within this batch to detect intra-batch duplicates
        var batchAssignments = new HashSet<(Guid leagueSeasonId, Guid roundId, Guid userId)>();

        foreach (var row in request.Rows)
        {
            var season = allSeasons.FirstOrDefault(s => s.SeasonNumber == row.SeasonNumber);
            if (season == null)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono sezonu o numerze {row.SeasonNumber}."));
                continue;
            }

            var league = allLeagues.FirstOrDefault(l => l.LeagueIdentifier == row.LeagueIdentifier);
            if (league == null)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono ligi '{row.LeagueIdentifier}'."));
                continue;
            }

            if (!leagueSeasonCache.TryGetValue(season.Id, out var leagueSeasons))
            {
                leagueSeasons = await unitOfWork.LeagueSeasonRepository.GetLeagueSeasonsForSeasonIdAsync(season.Id);
                leagueSeasonCache[season.Id] = leagueSeasons;
            }

            var leagueSeasonDto = leagueSeasons.FirstOrDefault(ls => ls.LeagueId == league.Id);
            if (leagueSeasonDto == null)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono ligo-sezonu dla sezonu {row.SeasonNumber} i ligi '{row.LeagueIdentifier}'."));
                continue;
            }

            if (!roundCache.TryGetValue(season.Id, out var rounds))
            {
                rounds = await unitOfWork.RoundRepository.GetRoundSummariesBySeasonIdAsync(season.Id);
                roundCache[season.Id] = rounds;
            }

            var roundDto = rounds.FirstOrDefault(r => r.RoundNumber == row.RoundNumber);
            if (roundDto == null)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono kolejki {row.RoundNumber} w sezonie {row.SeasonNumber}."));
                continue;
            }

            var userA = await unitOfWork.UserRepository.GetUserDetailByWcaIdAsync(row.WcaIdA);
            if (userA == null)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono użytkownika z WCA ID '{row.WcaIdA}'."));
                continue;
            }

            if (!await unitOfWork.LeagueSeasonUserRepository.ExistsAsync(
                    u => u.UserId == userA.Id && u.LeagueSeasonId == leagueSeasonDto.LeagueSeasonId))
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Użytkownik '{row.WcaIdA}' nie jest przypisany do ligo-sezonu S{row.SeasonNumber}/{row.LeagueIdentifier}."));
                continue;
            }

            if (batchAssignments.Contains((leagueSeasonDto.LeagueSeasonId, roundDto.Id, userA.Id)))
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Użytkownik '{row.WcaIdA}' pojawia się więcej niż raz w tej kolejce w pliku CSV."));
                continue;
            }

            Guid? userBId = null;
            if (!string.IsNullOrWhiteSpace(row.WcaIdB))
            {
                var userB = await unitOfWork.UserRepository.GetUserDetailByWcaIdAsync(row.WcaIdB);
                if (userB == null)
                {
                    results.Add(ImportRowResult.Fail(row.RowNumber, $"Nie znaleziono użytkownika z WCA ID '{row.WcaIdB}'."));
                    continue;
                }

                if (!await unitOfWork.LeagueSeasonUserRepository.ExistsAsync(
                        u => u.UserId == userB.Id && u.LeagueSeasonId == leagueSeasonDto.LeagueSeasonId))
                {
                    results.Add(ImportRowResult.Fail(row.RowNumber, $"Użytkownik '{row.WcaIdB}' nie jest przypisany do ligo-sezonu S{row.SeasonNumber}/{row.LeagueIdentifier}."));
                    continue;
                }

                if (batchAssignments.Contains((leagueSeasonDto.LeagueSeasonId, roundDto.Id, userB.Id)))
                {
                    results.Add(ImportRowResult.Fail(row.RowNumber, $"Użytkownik '{row.WcaIdB}' pojawia się więcej niż raz w tej kolejce w pliku CSV."));
                    continue;
                }

                userBId = userB.Id;
                batchAssignments.Add((leagueSeasonDto.LeagueSeasonId, roundDto.Id, userB.Id));
            }

            if (row.SolvesA.Count != Match.SOLVES_PER_MATCH)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Oczekiwano {Match.SOLVES_PER_MATCH} wyników dla gracza A, podano {row.SolvesA.Count}."));
                continue;
            }

            if (row.SolvesB.Count != Match.SOLVES_PER_MATCH)
            {
                results.Add(ImportRowResult.Fail(row.RowNumber, $"Oczekiwano {Match.SOLVES_PER_MATCH} wyników dla gracza B, podano {row.SolvesB.Count}."));
                continue;
            }

            batchAssignments.Add((leagueSeasonDto.LeagueSeasonId, roundDto.Id, userA.Id));
            results.Add(ImportRowResult.Ok(row.RowNumber,
                $"OK — S{row.SeasonNumber}/{row.LeagueIdentifier} Kol.{row.RoundNumber}: {row.WcaIdA} vs {row.WcaIdB ?? "BYE"}"));
        }

        if (results.Any(r => !r.Success))
            return new ImportResult { RowResults = results };

        // All rows valid — write in a single transaction
        await unitOfWork.BeginTransactionAsync();

        var matchByRoundCache = new Dictionary<Guid, List<Match>>();
        var affectedRoundIds = new HashSet<Guid>();

        for (var i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];

            var season = allSeasons.First(s => s.SeasonNumber == row.SeasonNumber);
            var league = allLeagues.First(l => l.LeagueIdentifier == row.LeagueIdentifier);
            var leagueSeason = leagueSeasonCache[season.Id].First(ls => ls.LeagueId == league.Id);
            var round = roundCache[season.Id].First(r => r.RoundNumber == row.RoundNumber);
            var userA = await unitOfWork.UserRepository.GetUserDetailByWcaIdAsync(row.WcaIdA);
            var userBId = string.IsNullOrWhiteSpace(row.WcaIdB)
                ? (Guid?)null
                : (await unitOfWork.UserRepository.GetUserDetailByWcaIdAsync(row.WcaIdB))!.Id;

            if (!matchByRoundCache.TryGetValue(round.Id, out var roundMatches))
            {
                roundMatches = (await unitOfWork.MatchRepository.GetMatchesByRoundIdAsync(round.Id)).ToList();
                matchByRoundCache[round.Id] = roundMatches;
            }

            var existingForA = roundMatches.FirstOrDefault(m =>
                m.LeagueSeasonId == leagueSeason.LeagueSeasonId &&
                (m.UserAId == userA!.Id || m.UserBId == userA!.Id));
            if (existingForA != null)
            {
                unitOfWork.MatchRepository.Delete(existingForA);
                roundMatches.Remove(existingForA);
            }

            if (userBId.HasValue)
            {
                var existingForB = roundMatches.FirstOrDefault(m =>
                    m.LeagueSeasonId == leagueSeason.LeagueSeasonId &&
                    (m.UserAId == userBId.Value || m.UserBId == userBId.Value));
                if (existingForB != null)
                {
                    unitOfWork.MatchRepository.Delete(existingForB);
                    roundMatches.Remove(existingForB);
                }
            }

            var match = Match.Create(leagueSeason.LeagueSeasonId, round.Id, userA!.Id, userBId);

            var solveDtosA = row.SolvesA.Select(s => new SolveDto { Result = s.ToString() }).ToList();
            var solveDtosB = row.SolvesB.Select(s => new SolveDto { Result = s.ToString() }).ToList();
            await MatchSolvesProcessor.ProcessAsync(unitOfWork, match, solveDtosA, solveDtosB);

            await unitOfWork.MatchRepository.AddAsync(match);
            affectedRoundIds.Add(round.Id);
        }

        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        foreach (var roundId in affectedRoundIds)
            await sender.Send(new RefreshRoundStandingsRequest(roundId), cancellationToken);

        return new ImportResult { RowResults = results };
    }
}
