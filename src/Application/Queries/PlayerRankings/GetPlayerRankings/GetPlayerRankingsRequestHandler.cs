using BldLeague.Application.Abstractions.Repositories;
using MediatR;

namespace BldLeague.Application.Queries.PlayerRankings.GetPlayerRankings;

public class GetPlayerRankingsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPlayerRankingsRequest, GetPlayerRankingsResponse>
{
    public async Task<GetPlayerRankingsResponse> Handle(GetPlayerRankingsRequest request, CancellationToken cancellationToken)
    {
        var rankings = await unitOfWork.PlayerRankingRepository.GetAllWithDetailsAsync();

        var singleRankings = rankings
            .Where(r => r.SingleRank.HasValue && r.BestSingle.HasValue)
            .OrderBy(r => r.SingleRank!.Value)
            .Select(r => new SingleRankingDto(
                r.SingleRank!.Value,
                r.User.FullName,
                r.BestSingle!.Value,
                $"Sezon {r.SingleRound!.Season.SeasonNumber} Kolejka {r.SingleRound.RoundNumber}"
            ))
            .ToList();

        var averageRankings = rankings
            .Where(r => r.AverageRank.HasValue && r.BestAverage.HasValue)
            .OrderBy(r => r.AverageRank!.Value)
            .Select(r => new AverageRankingDto(
                r.AverageRank!.Value,
                r.User.FullName,
                r.BestAverage!.Value,
                $"Sezon {r.AverageRound!.Season.SeasonNumber} Kolejka {r.AverageRound.RoundNumber}",
                r.AverageSolve1!.Value,
                r.AverageSolve2!.Value,
                r.AverageSolve3!.Value,
                r.AverageSolve4!.Value,
                r.AverageSolve5!.Value
            ))
            .ToList();

        return new GetPlayerRankingsResponse(singleRankings, averageRankings);
    }
}
