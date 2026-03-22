using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Matches.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetAll;

/// <summary>
/// Handles returning all matches projected into admin summary DTOs.
/// </summary>
public class GetAllMatchesRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllMatchesRequest, IReadOnlyCollection<MatchAdminSummaryDto>>
{
    public async Task<IReadOnlyCollection<MatchAdminSummaryDto>> Handle(GetAllMatchesRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.MatchRepository.GetAllAdminSummariesAsync();
    }
}
