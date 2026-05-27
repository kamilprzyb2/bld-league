using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Statistics.GetSolveDurationHistogram;
using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetMatchScoreDistribution;

public class GetMatchScoreDistributionRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetMatchScoreDistributionRequest, ScoreDistributionDto>
{
    // 5 solves + 1 best-single bonus = 6 points per match.
    // Buckets enumerate every (winner, loser) pair with winner >= loser >= 0,
    // winner <= 6, winner + loser <= 6 — 16 buckets total.
    // Ordering: winner desc, then loser asc.
    private static readonly (int Winner, int Loser)[] Buckets =
    [
        (6, 0),
        (5, 1), (5, 0),
        (4, 2), (4, 1), (4, 0),
        (3, 3), (3, 2), (3, 1), (3, 0),
        (2, 2), (2, 1), (2, 0),
        (1, 1), (1, 0),
        (0, 0),
    ];

    public async Task<ScoreDistributionDto> Handle(GetMatchScoreDistributionRequest request, CancellationToken cancellationToken)
    {
        var localToday = roundClock.LocalToday();
        var scores = await unitOfWork.StatisticsRepository.GetFinishedMatchScoresAsync(localToday);

        var counts = new int[Buckets.Length];
        foreach (var (scoreA, scoreB) in scores)
        {
            int winner = Math.Max(scoreA, scoreB);
            int loser = Math.Min(scoreA, scoreB);
            for (int i = 0; i < Buckets.Length; i++)
            {
                if (Buckets[i].Winner == winner && Buckets[i].Loser == loser)
                {
                    counts[i]++;
                    break;
                }
            }
        }

        var buckets = new HistogramBucketDto[Buckets.Length];
        for (int i = 0; i < Buckets.Length; i++)
            buckets[i] = new HistogramBucketDto($"{Buckets[i].Winner}:{Buckets[i].Loser}", counts[i]);

        return new ScoreDistributionDto(buckets);
    }
}
