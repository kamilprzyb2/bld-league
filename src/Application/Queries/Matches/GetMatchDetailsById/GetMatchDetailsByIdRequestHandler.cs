using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetMatchDetailsById;

/// <summary>
/// Handles retrieving full match details by ID, returning null if the match does not exist.
/// </summary>
public class GetMatchDetailsByIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetMatchDetailsByIdRequest, MatchDetailsDto?>
{
    public async Task<MatchDetailsDto?> Handle(GetMatchDetailsByIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.MatchRepository.GetMatchDetailsByIdAsync(request.Id);
    }
}
