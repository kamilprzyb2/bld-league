using BldLeague.Application.Abstractions.Repositories;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetByLeagueSeasonId;

/// <summary>
/// Handles retrieving all user summaries enrolled in a given league season, including subleague group.
/// </summary>
public class GetUsersByLeagueSeasonIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUsersByLeagueSeasonIdRequest, IReadOnlyCollection<LeagueSeasonUserDto>>
{
    public async Task<IReadOnlyCollection<LeagueSeasonUserDto>> Handle(GetUsersByLeagueSeasonIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.LeagueSeasonUserRepository.GetUsersByLeagueSeasonIdAsync(request.LeagueSeasonId);
    }
}
