using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Users.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetByLeagueSeasonId;

/// <summary>
/// Handles retrieving all user summaries enrolled in a given league season.
/// </summary>
public class GetUsersByLeagueSeasonIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUsersByLeagueSeasonIdRequest, IReadOnlyCollection<UserSummaryDto>>
{
    public async Task<IReadOnlyCollection<UserSummaryDto>> Handle(GetUsersByLeagueSeasonIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.LeagueSeasonUserRepository.GetUsersByLeagueSeasonIdAsync(request.LeagueSeasonId);
    }
}
