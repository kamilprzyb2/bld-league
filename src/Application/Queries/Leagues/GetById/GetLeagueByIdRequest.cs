using BldLeague.Application.Queries.Leagues.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Leagues.GetById;

/// <summary>
/// Request to retrieve a single league summary DTO by its unique identifier.
/// </summary>
public class GetLeagueByIdRequest : IRequest<LeagueSummaryDto?>
{
    public Guid LeagueId { get; set; }
}
