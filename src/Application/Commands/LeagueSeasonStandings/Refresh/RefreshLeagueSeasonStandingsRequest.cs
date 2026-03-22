using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonStandings.Refresh;

/// <summary>
/// Request to recalculate and persist cumulative standings for a specific league season.
/// </summary>
public class RefreshLeagueSeasonStandingsRequest(Guid leagueSeasonId) : IRequest<CommandResult>
{
    public Guid LeagueSeasonId { get; set; } = leagueSeasonId;
}
