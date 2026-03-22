using BldLeague.Application.Abstractions.Repositories;
using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetMaxRoundNumberBySeasonId;

/// <summary>
/// Handles retrieving the latest round number across all rounds, returning null if no rounds exist.
/// </summary>
public class GetMaxRoundNumberBySeasonIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetMaxRoundNumberBySeasonIdRequest, int?>
{
    public async Task<int?> Handle(GetMaxRoundNumberBySeasonIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.RoundRepository.GetLatestRoundNumberAsync();
    }
}
