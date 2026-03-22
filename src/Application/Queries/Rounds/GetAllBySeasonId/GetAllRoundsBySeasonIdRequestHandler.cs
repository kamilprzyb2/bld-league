using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetAllBySeasonId;

/// <summary>
/// Handles retrieving all round summaries for a given season ID.
/// </summary>
public class GetAllRoundsBySeasonIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllRoundsBySeasonIdRequest, IReadOnlyCollection<RoundSummaryDto>>
{
    public async Task<IReadOnlyCollection<RoundSummaryDto>> Handle(GetAllRoundsBySeasonIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.RoundRepository.GetRoundSummariesBySeasonIdAsync(request.SeasonId);
    }
}
