using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetById;

/// <summary>
/// Handles retrieving a round summary DTO by ID, returning null if not found.
/// </summary>
public class GetRoundByIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetRoundByIdRequest, RoundSummaryDto?>
{
    public async Task<RoundSummaryDto?> Handle(GetRoundByIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.RoundRepository.GetSummaryByIdAsync(request.Id);
    }
}
