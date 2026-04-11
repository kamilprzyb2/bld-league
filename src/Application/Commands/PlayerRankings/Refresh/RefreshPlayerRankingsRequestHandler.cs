using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.PlayerRankings.Refresh;

public class RefreshPlayerRankingsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshPlayerRankingsRequest, CommandResult>
{
    public async Task<CommandResult> Handle(RefreshPlayerRankingsRequest request, CancellationToken cancellationToken)
    {
        var allStandings = await unitOfWork.RoundStandingRepository.GetAllAsync();

        var byUser = allStandings.GroupBy(rs => rs.UserId).ToList();

        var usersWithBestSingle = byUser
            .Select(g =>
            {
                var validSingles = g.Where(rs => rs.Best.IsValid).ToList();
                if (validSingles.Count == 0)
                    return (UserId: g.Key, BestSingle: (BldLeague.Domain.ValueObjects.SolveResult?)null, SingleRoundId: (Guid?)null);
                var best = validSingles.OrderBy(rs => rs.Best.Centiseconds).First();
                return (UserId: g.Key, BestSingle: (BldLeague.Domain.ValueObjects.SolveResult?)best.Best, SingleRoundId: (Guid?)best.RoundId);
            })
            .ToList();

        var usersWithBestAverage = byUser
            .Select(g =>
            {
                var validAverages = g.Where(rs => rs.Average.IsValid).ToList();
                if (validAverages.Count == 0)
                    return (UserId: g.Key, BestAverage: (BldLeague.Domain.ValueObjects.SolveResult?)null, AverageRoundId: (Guid?)null, Standing: (RoundStanding?)null);
                var best = validAverages.OrderBy(rs => rs.Average.Centiseconds).First();
                return (UserId: g.Key, BestAverage: (BldLeague.Domain.ValueObjects.SolveResult?)best.Average, AverageRoundId: (Guid?)best.RoundId, Standing: (RoundStanding?)best);
            })
            .ToList();

        // Assign single ranks (WCA tie logic)
        var singleRanked = usersWithBestSingle
            .Where(u => u.BestSingle.HasValue)
            .OrderBy(u => u.BestSingle!.Value.Centiseconds)
            .ToList();

        var singleRanks = new Dictionary<Guid, int>();
        int previousSingleCs = -1;
        int previousSingleRank = 1;
        for (int i = 0; i < singleRanked.Count; i++)
        {
            var cs = singleRanked[i].BestSingle!.Value.Centiseconds;
            int rank;
            if (i == 0)
            {
                rank = 1;
            }
            else if (cs == previousSingleCs)
            {
                rank = previousSingleRank;
            }
            else
            {
                rank = i + 1;
            }
            previousSingleCs = cs;
            previousSingleRank = rank;
            singleRanks[singleRanked[i].UserId] = rank;
        }

        // Assign average ranks (WCA tie logic)
        var averageRanked = usersWithBestAverage
            .Where(u => u.BestAverage.HasValue)
            .OrderBy(u => u.BestAverage!.Value.Centiseconds)
            .ToList();

        var averageRanks = new Dictionary<Guid, int>();
        int previousAverageCs = -1;
        int previousAverageRank = 1;
        for (int i = 0; i < averageRanked.Count; i++)
        {
            var cs = averageRanked[i].BestAverage!.Value.Centiseconds;
            int rank;
            if (i == 0)
            {
                rank = 1;
            }
            else if (cs == previousAverageCs)
            {
                rank = previousAverageRank;
            }
            else
            {
                rank = i + 1;
            }
            previousAverageCs = cs;
            previousAverageRank = rank;
            averageRanks[averageRanked[i].UserId] = rank;
        }

        // Build lookup maps
        var singleMap = usersWithBestSingle.ToDictionary(u => u.UserId);
        var averageMap = usersWithBestAverage.ToDictionary(u => u.UserId);

        var allUserIds = byUser.Select(g => g.Key).ToList();

        var existingRankings = await unitOfWork.PlayerRankingRepository.GetAllAsync();
        var existingByUserId = existingRankings.ToDictionary(r => r.UserId);

        var toAdd = new List<PlayerRanking>();
        var toUpdate = new List<PlayerRanking>();

        foreach (var userId in allUserIds)
        {
            bool isNew = !existingByUserId.TryGetValue(userId, out var ranking);
            if (isNew)
                ranking = PlayerRanking.Create(userId);

            var singleEntry = singleMap[userId];
            ranking.BestSingle = singleEntry.BestSingle;
            ranking.SingleRoundId = singleEntry.SingleRoundId;
            ranking.SingleRank = singleRanks.TryGetValue(userId, out var sr) ? sr : null;

            var avgEntry = averageMap[userId];
            ranking.BestAverage = avgEntry.BestAverage;
            ranking.AverageRoundId = avgEntry.AverageRoundId;
            ranking.AverageRank = averageRanks.TryGetValue(userId, out var ar) ? ar : null;

            if (avgEntry.Standing != null)
            {
                ranking.AverageSolve1 = avgEntry.Standing.Solve1;
                ranking.AverageSolve2 = avgEntry.Standing.Solve2;
                ranking.AverageSolve3 = avgEntry.Standing.Solve3;
                ranking.AverageSolve4 = avgEntry.Standing.Solve4;
                ranking.AverageSolve5 = avgEntry.Standing.Solve5;
            }
            else
            {
                ranking.AverageSolve1 = null;
                ranking.AverageSolve2 = null;
                ranking.AverageSolve3 = null;
                ranking.AverageSolve4 = null;
                ranking.AverageSolve5 = null;
            }

            if (isNew)
                toAdd.Add(ranking);
            else
                toUpdate.Add(ranking);
        }

        foreach (var ranking in toUpdate)
            unitOfWork.PlayerRankingRepository.Update(ranking);
        await unitOfWork.PlayerRankingRepository.AddRangeAsync(toAdd);

        await unitOfWork.SaveAsync();

        return CommandResult.Ok("Zaktualizowano rankingi zawodników");
    }
}
