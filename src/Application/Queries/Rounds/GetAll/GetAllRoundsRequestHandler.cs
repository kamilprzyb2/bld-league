using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Rounds.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetAll;

/// <summary>
/// Handles returning all rounds projected into admin summary DTOs.
/// </summary>
public class GetAllRoundsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllRoundsRequest, IReadOnlyCollection<RoundAdminSummaryDto>>
{
    public async Task<IReadOnlyCollection<RoundAdminSummaryDto>> Handle(GetAllRoundsRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.RoundRepository.GetAllRoundSummariesAsync();
    }
}
