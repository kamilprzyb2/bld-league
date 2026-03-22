using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetAllBySeasonId;

/// <summary>
/// Request to retrieve all rounds for a specific season as summary DTOs.
/// </summary>
public class GetAllRoundsBySeasonIdRequest(Guid seasonId) : IRequest<IReadOnlyCollection<RoundSummaryDto>>
{
    public Guid SeasonId { get; set; } = seasonId;
}
