using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Seasons.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Seasons.GetById;

/// <summary>
/// Handles retrieving a season summary DTO by ID, returning null if not found.
/// </summary>
public class GetSeasonByIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSeasonByIdRequest, SeasonSummaryDto?>
{
    public async Task<SeasonSummaryDto?> Handle(GetSeasonByIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.SeasonRepository.GetSummaryByIdAsync(request.Id);
    }
}
