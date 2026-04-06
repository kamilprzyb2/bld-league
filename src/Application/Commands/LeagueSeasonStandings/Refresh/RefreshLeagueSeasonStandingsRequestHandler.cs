using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonStandings.Refresh;

/// <summary>
/// Handles recalculating cumulative standings for all users in a league season, including match points, bonus points, and best solve.
/// </summary>
public class RefreshLeagueSeasonStandingsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshLeagueSeasonStandingsRequest, CommandResult>
{
    public async Task<CommandResult> Handle(RefreshLeagueSeasonStandingsRequest request, CancellationToken cancellationToken)
    {
        var leagueSeason = await unitOfWork.LeagueSeasonRepository.GetByIdAsync(request.LeagueSeasonId);
        if (leagueSeason == null)
        {
            return CommandResult.FailGeneral(
                $"Nie znaleziono ligo-sezonu z ID: {request.LeagueSeasonId}");
        }

        var repository = unitOfWork.LeagueSeasonStandingRepository;

        var users = await unitOfWork.LeagueSeasonUserRepository.GetUsersByLeagueSeasonIdAsync(request.LeagueSeasonId);
        Dictionary<Guid, Standing> standings = []; // userId -> standing
        Dictionary<Guid, int> subleagueGroups = users.ToDictionary(u => u.Id, u => u.SubleagueGroup);

        List<LeagueSeasonStanding> toUpdate =
            (await unitOfWork.LeagueSeasonStandingRepository.GetStandingsByLeagueSeasonIdAsync(request.LeagueSeasonId))
            .ToList();

        List<LeagueSeasonStanding> toAdd = [];


        // 1. Calculate big points and small points
        var matches = await unitOfWork.MatchRepository
            .GetMatchesByLeagueSeasonAsync(request.LeagueSeasonId);
        foreach (var user in users)
        {
            standings.Add(user.Id, new Standing());
        }

        foreach (var match in matches)
        {
            if (match.UserBId == null)
                continue;

            var userAStanding = standings[match.UserAId];
            var userBStanding = standings[match.UserBId.Value];

            userAStanding.MatchesPlayed++;
            userBStanding.MatchesPlayed++;

            if (match.UserAScore > match.UserBScore)
            {
                userAStanding.MatchesWon++;
                userBStanding.MatchesLost++;

                userAStanding.BigPoints+=2;
            }
            else if (match.UserAScore < match.UserBScore)
            {
                userBStanding.MatchesWon++;
                userAStanding.MatchesLost++;

                userBStanding.BigPoints+=2;
            }
            else // tie
            {
                // If both players scored 0, it's a mutual DNS/DNF — no points awarded
                if (match.UserAScore == 0 && match.UserBScore == 0)
                {
                    userAStanding.MatchesLost++;
                    userBStanding.MatchesLost++;
                }
                else
                {
                    userAStanding.MatchesTied++;
                    userBStanding.MatchesTied++;

                    userAStanding.BigPoints++;
                    userBStanding.BigPoints++;
                }
            }

            userAStanding.SmallPointsGained += match.UserAScore;
            userAStanding.SmallPointsLost += match.UserBScore;
            userAStanding.SmallPointsBalance += match.UserAScore - match.UserBScore;

            userBStanding.SmallPointsGained += match.UserBScore;
            userBStanding.SmallPointsLost += match.UserAScore;
            userBStanding.SmallPointsBalance += match.UserBScore - match.UserAScore;
        }

        // 2. Calculate bonus points
        var bonusStandings = await unitOfWork.RoundStandingRepository.GetBonusPointsForLeagueSeasonAsync(
            leagueSeason.LeagueId, leagueSeason.SeasonId);

        foreach (var standing in bonusStandings)
        {
            var userId = standing.Item1;
            standings[userId].BonusPoints =  standing.Item2;
        }

        // 3. Calculate best solve
        var bestSolves = await unitOfWork.SolveRepository.GetBestSolvesForLeagueSeason(
            request.LeagueSeasonId);

        foreach (var bestSolve in bestSolves)
        {
            var userId = bestSolve.Item1;
            standings[userId].Best =  bestSolve.Item2;
        }

        // 4. Calculate places and update collections
        var sorted = standings
            .OrderBy(s => subleagueGroups.GetValueOrDefault(s.Key, 0))
            .ThenByDescending(s => s.Value.BigPoints)
            .ThenByDescending(s => s.Value.BonusPoints)
            .ThenByDescending(s => s.Value.SmallPointsBalance)
            .ThenByDescending(s => s.Value.Best)
            .ToList();

        int previous = 1;
        for (int i = 0; i < sorted.Count; i++)
        {
            int place = i + 1;
            var standing = sorted[i].Value;

            var sameGroup = i > 0 &&
                subleagueGroups.GetValueOrDefault(sorted[i].Key, 0) == subleagueGroups.GetValueOrDefault(sorted[i - 1].Key, 0);

            if (sameGroup &&
                standing.BigPoints == sorted[i-1].Value.BigPoints &&
                standing.BonusPoints == sorted[i-1].Value.BonusPoints &&
                standing.SmallPointsBalance == sorted[i-1].Value.SmallPointsBalance &&
                standing.Best == sorted[i-1].Value.Best) // somehow tied within same group
            {
                place = previous;
            }
            else
            {
                previous = place;
            }

            var existing = toUpdate.FirstOrDefault(lss => lss.UserId == sorted[i].Key);
            var best = standing.Best == int.MaxValue ? SolveResult.Dnf() : SolveResult.FromCentiseconds(standing.Best);

            if (existing != null)
            {
                existing.Place = place;
                existing.MatchesPlayed = standing.MatchesPlayed;
                existing.MatchesWon = standing.MatchesWon;
                existing.MatchesTied = standing.MatchesTied;
                existing.MatchesLost = standing.MatchesLost;
                existing.BigPoints = standing.BigPoints;
                existing.BonusPoints = standing.BonusPoints;
                existing.SmallPointsGained = standing.SmallPointsGained;
                existing.SmallPointsLost = standing.SmallPointsLost;
                existing.SmallPointsBalance = standing.SmallPointsBalance;
                existing.Best = best;
            }
            else
            {
                toAdd.Add(LeagueSeasonStanding.Create(
                    leagueSeason.Id, sorted[i].Key,
                    place, standing.MatchesPlayed, standing.MatchesWon,
                    standing.MatchesTied, standing.MatchesLost, standing.BigPoints,
                    standing.BonusPoints, standing.SmallPointsGained, standing.SmallPointsLost,
                    standing.SmallPointsBalance, best));
            }
        }

        await unitOfWork.BeginTransactionAsync();
        foreach (var lss in toUpdate)
        {
            repository.Update(lss);
        }
        await repository.AddRangeAsync(toAdd);
        await unitOfWork.CommitTransactionAsync();
        await unitOfWork.SaveAsync();

        return CommandResult.Ok("Zaktualizowano klasyfikację ligo-sezonu.");
    }

    private class Standing
    {
        public int MatchesPlayed { get; set; }
        public int MatchesWon { get; set; }
        public int MatchesTied { get; set; }
        public int MatchesLost { get; set; }
        public int BigPoints { get; set; }
        public int BonusPoints { get; set; }
        public int SmallPointsGained { get; set; }
        public int SmallPointsLost { get; set; }
        public int SmallPointsBalance { get; set; }
        public int Best { get; set; } = int.MaxValue;
    }
}
