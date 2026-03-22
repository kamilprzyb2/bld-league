using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Leagues.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Leagues.GetAll;

/// <summary>
/// Handles returning all leagues projected into summary DTOs.
/// </summary>
public class GetAllLeaguesRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllLeaguesRequest, IReadOnlyCollection<LeagueSummaryDto>>
{
    public async Task<IReadOnlyCollection<LeagueSummaryDto>> Handle(GetAllLeaguesRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.LeagueRepository.GetAllSummariesAsync();
    }
}
