using BldLeague.Application.Queries.LeagueSeasons.GetDetail;
using MediatR;

namespace BldLeague.Application.Queries.LeagueSeasons.GetDetail;

/// <summary>
/// Request to retrieve the full standings detail for a league season identified by season ID and league ID.
/// </summary>
public class GetLeagueSeasonDetailRequest(Guid seasonId, Guid leagueId)
    : IRequest<LeagueSeasonDetailDto?>
{
    public Guid SeasonId { get; } = seasonId;
    public Guid LeagueId { get; } = leagueId;
}
