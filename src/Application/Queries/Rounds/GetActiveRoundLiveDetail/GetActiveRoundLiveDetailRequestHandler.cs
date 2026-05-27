using BldLeague.Application.Abstractions.Repositories;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetActiveRoundLiveDetail;

/// <summary>
/// Handles retrieving the live snapshot for an active round, returning null if the round is not found.
/// </summary>
public class GetActiveRoundLiveDetailRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetActiveRoundLiveDetailRequest, ActiveRoundLiveDetailDto?>
{
    public async Task<ActiveRoundLiveDetailDto?> Handle(
        GetActiveRoundLiveDetailRequest request,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.RoundRepository
            .GetActiveRoundLiveDetailAsync(request.SeasonId, request.RoundNumber);
    }
}
