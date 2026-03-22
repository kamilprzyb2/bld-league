using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetById;

/// <summary>
/// Request to retrieve a single round summary DTO by its unique identifier.
/// </summary>
public class GetRoundByIdRequest(Guid id) : IRequest<RoundSummaryDto?>
{
    public Guid Id { get; set; } = id;
}
