using BldLeague.Application.Queries.Seasons.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Seasons.GetById;

/// <summary>
/// Request to retrieve a single season summary DTO by its unique identifier.
/// </summary>
public class GetSeasonByIdRequest(Guid id) : IRequest<SeasonSummaryDto?>
{
    public Guid Id { get; set; } = id;
}
