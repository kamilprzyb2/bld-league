using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.RoundStandings.RefreshAll;

/// <summary>
/// Request to recalculate and persist standings for all rounds.
/// </summary>
public class RefreshAllRoundStandingsRequest : IRequest<CommandResult>;
