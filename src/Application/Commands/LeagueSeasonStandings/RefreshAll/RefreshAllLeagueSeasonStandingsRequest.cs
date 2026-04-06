using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonStandings.RefreshAll;

/// <summary>
/// Request to recalculate and persist cumulative standings for all league seasons.
/// </summary>
public class RefreshAllLeagueSeasonStandingsRequest : IRequest<CommandResult>;
