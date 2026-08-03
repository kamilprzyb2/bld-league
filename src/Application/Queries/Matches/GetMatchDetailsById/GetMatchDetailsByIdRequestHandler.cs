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
        var details = await unitOfWork.MatchRepository.GetMatchDetailsByIdAsync(request.Id);
        if (details == null)
            return null;

        var standingA = await unitOfWork.RoundStandingRepository.GetByRoundAndUserAsync(details.RoundId, details.UserAId);
        if (standingA != null)
        {
            details.UserABestRecord = standingA.BestRecord;
            details.UserAAverageRecord = standingA.AverageRecord;
        }

        if (details.UserBId.HasValue)
        {
            var standingB = await unitOfWork.RoundStandingRepository.GetByRoundAndUserAsync(details.RoundId, details.UserBId.Value);
            if (standingB != null)
            {
                details.UserBBestRecord = standingB.BestRecord;
                details.UserBAverageRecord = standingB.AverageRecord;
            }
        }

        return details;
    }
}
