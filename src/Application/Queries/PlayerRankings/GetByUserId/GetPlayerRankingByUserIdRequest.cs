using MediatR;

namespace BldLeague.Application.Queries.PlayerRankings.GetByUserId;

public class GetPlayerRankingByUserIdRequest : IRequest<PlayerRankingDto?>
{
    public Guid UserId { get; set; }
}
