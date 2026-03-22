using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.RoundStandings.Refresh;

/// <summary>
/// Request to recalculate and persist standings for all participants in a specific round.
/// </summary>
public class RefreshRoundStandingsRequest(Guid roundId) : IRequest<CommandResult>
{
    public Guid RoundId { get; set; } = roundId;
}
