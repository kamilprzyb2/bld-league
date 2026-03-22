using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.LeagueSeasons.GetById;

/// <summary>
/// Request to retrieve a single league season summary DTO by its unique identifier.
/// </summary>
public class GetLeagueSeasonByIdRequest(Guid leagueSeasonId) : IRequest<LeagueSeasonDto?>
{
    public Guid LeagueSeasonId { get; set; } = leagueSeasonId;
}
