using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetMatchDetailsById;

/// <summary>
/// Request to retrieve the full details of a match, including solves and scrambles, by its unique identifier.
/// </summary>
public class GetMatchDetailsByIdRequest(Guid id) : IRequest<MatchDetailsDto?>
{
    public Guid Id { get; set; } = id;
}
