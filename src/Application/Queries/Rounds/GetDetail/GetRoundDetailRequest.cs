using BldLeague.Application.Queries.Rounds.GetDetail;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetDetail;

/// <summary>
/// Request to retrieve the full results and standings detail for a round identified by season ID and round number.
/// </summary>
public class GetRoundDetailRequest(Guid seasonId, int roundNumber) : IRequest<RoundDetailDto?>
{
    public Guid SeasonId { get; } = seasonId;
    public int RoundNumber { get; } = roundNumber;
}
