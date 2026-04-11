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
        var bestSingles = await unitOfWork.RoundStandingRepository.GetBestSinglePerUserAsync();
        var bestAverages = await unitOfWork.RoundStandingRepository.GetBestAveragePerUserAsync();

        var averageByUserId = bestAverages.ToDictionary(a => a.UserId);

        // Assign single ranks (WCA tie logic)
        var singleRanked = bestSingles.OrderBy(s => s.Best.Centiseconds).ToList();
        var singleRanks = new Dictionary<Guid, int>();
        int previousSingleCs = -1;
        int previousSingleRank = 1;
        for (int i = 0; i < singleRanked.Count; i++)
        {
            var cs = singleRanked[i].Best.Centiseconds;
            int rank = (i == 0) ? 1 : (cs == previousSingleCs ? previousSingleRank : i + 1);
            previousSingleCs = cs;
            previousSingleRank = rank;
            singleRanks[singleRanked[i].UserId] = rank;
        }

        // Assign average ranks (WCA tie logic)
        var averageRanked = bestAverages.OrderBy(a => a.Average.Centiseconds).ToList();
        var averageRanks = new Dictionary<Guid, int>();
        int previousAverageCs = -1;
        int previousAverageRank = 1;
        for (int i = 0; i < averageRanked.Count; i++)
        {
            var cs = averageRanked[i].Average.Centiseconds;
            int rank = (i == 0) ? 1 : (cs == previousAverageCs ? previousAverageRank : i + 1);
            previousAverageCs = cs;
            previousAverageRank = rank;
            averageRanks[averageRanked[i].UserId] = rank;
        }

        var rankings = bestSingles.Select(s =>
        {
            var ranking = PlayerRanking.Create(s.UserId);
            ranking.BestSingle = s.Best;
            ranking.SingleRoundId = s.RoundId;
            ranking.SingleRank = singleRanks[s.UserId];

            if (averageByUserId.TryGetValue(s.UserId, out var avg))
            {
                ranking.BestAverage = avg.Average;
                ranking.AverageRoundId = avg.RoundId;
                ranking.AverageRank = averageRanks[s.UserId];
                ranking.AverageSolve1 = avg.Solve1;
                ranking.AverageSolve2 = avg.Solve2;
                ranking.AverageSolve3 = avg.Solve3;
                ranking.AverageSolve4 = avg.Solve4;
                ranking.AverageSolve5 = avg.Solve5;
            }

            return ranking;
        }).ToList();

        await unitOfWork.BeginTransactionAsync();
        await unitOfWork.PlayerRankingRepository.DeleteAllAsync();
        await unitOfWork.PlayerRankingRepository.AddRangeAsync(rankings);
        await unitOfWork.SaveAsync();
        await unitOfWork.CommitTransactionAsync();

        return CommandResult.Ok("Zaktualizowano rankingi zawodników");
    }
}
