using BldLeague.Application.Abstractions.Repositories;
using MediatR;

namespace BldLeague.Application.Queries.LeagueSeasons.GetUserLeagueIdForSeason;

public class GetUserLeagueIdForSeasonRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserLeagueIdForSeasonRequest, Guid?>
{
    public Task<Guid?> Handle(GetUserLeagueIdForSeasonRequest request, CancellationToken cancellationToken)
        => unitOfWork.LeagueSeasonUserRepository.GetUserLeagueIdForSeasonAsync(request.UserId, request.SeasonId);
}
