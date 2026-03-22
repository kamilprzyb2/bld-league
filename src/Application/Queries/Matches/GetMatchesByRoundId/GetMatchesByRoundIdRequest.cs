using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetMatchesByRoundId;

/// <summary>
/// Request to retrieve all matches belonging to a specific round by its ID.
/// </summary>
public class GetMatchesByRoundIdRequest(Guid roundId) : IRequest<IReadOnlyCollection<Match>>
{
    public Guid RoundId { get; set; } = roundId;
}
