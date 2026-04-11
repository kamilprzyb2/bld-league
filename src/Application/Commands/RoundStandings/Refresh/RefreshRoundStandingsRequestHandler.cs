using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Commands.LeagueSeasonStandings.Refresh;
using BldLeague.Application.Commands.PlayerRankings.Refresh;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Commands.RoundStandings.Refresh;

/// <summary>
/// Handles recalculating round standings from match results, assigning places and bonus points, then triggering league season standings refresh.
/// </summary>
public class RefreshRoundStandingsRequestHandler(IUnitOfWork unitOfWork, ISender sender)
    : IRequestHandler<RefreshRoundStandingsRequest, CommandResult>
{
    public async Task<CommandResult> Handle(RefreshRoundStandingsRequest request, CancellationToken cancellationToken)
    {
        var round = await unitOfWork.RoundRepository.GetByIdAsync(request.RoundId);
        if (round == null)
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono rundy z ID: {request.RoundId}");
        }

        if (round.EndDate >= DateTime.UtcNow)
        {
            return CommandResult.FailGeneral(
                $"Nie można odświeżyć klasyfikacji niezakończonej kolejki (kolejka {round.RoundNumber}).");
        }

        var repository = unitOfWork.RoundStandingRepository;

        List<RoundStanding> toUpdate = (await repository.GetRoundStandingsByRoundId(request.RoundId)).ToList();
        List<RoundStanding> toAdd = [];

        var matches = await unitOfWork.MatchRepository.GetMatchesByRoundIdAsync(request.RoundId);

        Dictionary<Guid, int> timedResults = [];
        Dictionary<Guid, List<SolveResult>> userSolves = [];
        Dictionary<Guid, SolveResult> userBests = [];
        Dictionary<Guid, SolveResult> userAverages = [];
        Dictionary<Guid, Guid> userLeagues = [];

        foreach (var match in matches)
        {
            if (!match.UserABest.IsDns)
            {
                timedResults.Add(match.UserAId, match.UserABest.IsValid ? match.UserABest.Centiseconds : int.MaxValue);
                userSolves.Add(match.UserAId,
                    match.Solves
                        .Where(m=>m.UserId == match.UserAId)
                        .OrderBy(s=>s.Index)
                        .Select(s=>s.Result)
                        .ToList());
                userBests.Add(match.UserAId, match.UserABest);
                userAverages.Add(match.UserAId, match.UserAAverage);
                userLeagues.Add(match.UserAId, match.LeagueSeason.LeagueId);
            }

            if (match.UserBId.HasValue && !match.UserBBest.IsDns)
            {
                var userBId = match.UserBId.Value;
                timedResults.Add(userBId, match.UserBBest.IsValid ? match.UserBBest.Centiseconds : int.MaxValue);
                userSolves.Add(userBId,
                    match.Solves
                        .Where(m=>m.UserId == match.UserBId)
                        .OrderBy(s=>s.Index)
                        .Select(s=>s.Result)
                        .ToList());
                userBests.Add(userBId, match.UserBBest);
                userAverages.Add(userBId, match.UserBAverage);
                userLeagues.Add(userBId, match.LeagueSeason.LeagueId);
            }
        }

        var sorted = timedResults.OrderBy(kvp => kvp.Value).ToList();

        int previous = 1;
        for (int i = 0; i < timedResults.Count; i++)
        {
            var userId = sorted[i].Key;
            var timedResult = sorted[i].Value;

            int place;

            if (timedResult == int.MaxValue)
            {
                place = int.MaxValue; // not classified (DNF)
            }
            else
            {
                place = i + 1;
                if (i > 0 && timedResult == sorted[i - 1].Value) // tied
                {
                    place = previous;
                }
                else
                {
                    previous = place;
                }
            }

            var points = 0;
            if (place <= 7)
                points = 50 - 2 * (place - 1);
            else if (place <= 44)
                points = 37 - (place - 8);

            var existing = toUpdate.FirstOrDefault(rs => rs.UserId == userId);

            var solves = userSolves[userId];
            if (solves.Count != Match.SOLVES_PER_MATCH)
            {
                return CommandResult.FailGeneral(
                    $"Niezgodna liczba ułożeń użytkownika {userId} - {solves.Count}" +
                    $"\nOczekiwano {Match.SOLVES_PER_MATCH}");
            }

            if (existing != null)
            {
                existing.Place = place;
                existing.Points = points;

                existing.Solve1 = solves[0];
                existing.Solve2 = solves[1];
                existing.Solve3 = solves[2];
                existing.Solve4 = solves[3];
                existing.Solve5 = solves[4];

                existing.Best = userBests[userId];
                existing.Average = userAverages[userId];
            }
            else
            {
                toAdd.Add(RoundStanding.Create(
                    userId, request.RoundId, userLeagues[userId], points, place,
                    solves[0], solves[1], solves[2], solves[3], solves[4],
                    userBests[userId], userAverages[userId]));
            }
        }

        await unitOfWork.BeginTransactionAsync();
        foreach (var rs in toUpdate)
        {
            repository.Update(rs);
        }
        await repository.AddRangeAsync(toAdd);
        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        var leagueSeasons = await unitOfWork.LeagueSeasonRepository.GetLeagueSeasonsForSeasonIdAsync(round.SeasonId);
        foreach (var ls in leagueSeasons)
            await sender.Send(new RefreshLeagueSeasonStandingsRequest(ls.LeagueSeasonId), cancellationToken);

        await sender.Send(new RefreshPlayerRankingsRequest(), cancellationToken);

        return CommandResult.Ok("Zaktualizowano klasyfikację kolejki");
    }
}
