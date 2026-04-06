using MediatR;

namespace BldLeague.Application.Queries.Users.GetByLeagueSeasonId;

/// <summary>
/// Request to retrieve all users enrolled in a specific league season, including their subleague group.
/// </summary>
public class GetUsersByLeagueSeasonIdRequest(Guid leagueSeasonId) : IRequest<IReadOnlyCollection<LeagueSeasonUserDto>>
{
    public Guid LeagueSeasonId { get; set; } = leagueSeasonId;
}
