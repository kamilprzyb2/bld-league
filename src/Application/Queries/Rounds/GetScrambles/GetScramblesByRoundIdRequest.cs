using BldLeague.Application.Queries.Rounds.GetScrambles;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetScrambles;

/// <summary>
/// Request to retrieve all scrambles for a specific round as a collection of DTOs.
/// </summary>
public class GetScramblesByRoundIdRequest(Guid roundId) : IRequest<IReadOnlyCollection<ScrambleDto>>
{
    public Guid RoundId { get; } = roundId;
}
