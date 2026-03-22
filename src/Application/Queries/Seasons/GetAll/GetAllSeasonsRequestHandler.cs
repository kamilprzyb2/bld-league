using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Seasons.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Seasons.GetAll;

/// <summary>
/// Handles returning all seasons projected into summary DTOs.
/// </summary>
public class GetAllSeasonsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllSeasonsRequest, IReadOnlyCollection<SeasonSummaryDto>>
{
    public async Task<IReadOnlyCollection<SeasonSummaryDto>> Handle(GetAllSeasonsRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.SeasonRepository.GetAllSummariesAsync();
    }
}
