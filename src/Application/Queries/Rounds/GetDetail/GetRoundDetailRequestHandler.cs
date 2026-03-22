using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Rounds.GetDetail;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetDetail;

/// <summary>
/// Handles retrieving round detail including standings and scrambles, returning null if not found.
/// </summary>
public class GetRoundDetailRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetRoundDetailRequest, RoundDetailDto?>
{
    public async Task<RoundDetailDto?> Handle(GetRoundDetailRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.RoundRepository.GetRoundDetailAsync(request.SeasonId, request.RoundNumber);
    }
}
