using BldLeague.Application.Abstractions.Repositories;
using MediatR;

namespace BldLeague.Application.Queries.PlayerRankings.GetByUserId;

public class GetPlayerRankingByUserIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPlayerRankingByUserIdRequest, PlayerRankingDto?>
{
    public async Task<PlayerRankingDto?> Handle(GetPlayerRankingByUserIdRequest request, CancellationToken cancellationToken)
    {
        var ranking = await unitOfWork.PlayerRankingRepository.GetByUserIdAsync(request.UserId);
        if (ranking == null)
            return null;

        return new PlayerRankingDto(
            ranking.BestSingle,
            ranking.SingleRank,
            ranking.BestAverage,
            ranking.AverageRank
        );
    }
}
