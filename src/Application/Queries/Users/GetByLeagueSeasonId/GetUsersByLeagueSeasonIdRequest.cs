using BldLeague.Application.Queries.Users.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetByLeagueSeasonId;

/// <summary>
/// Request to retrieve all users enrolled in a specific league season as summary DTOs.
/// </summary>
public class GetUsersByLeagueSeasonIdRequest(Guid leagueSeasonId) : IRequest<IReadOnlyCollection<UserSummaryDto>>
{
    public Guid LeagueSeasonId { get; set; } = leagueSeasonId;
}
