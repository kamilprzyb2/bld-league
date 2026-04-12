using BldLeague.Application.Abstractions.Repositories;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetSeasonHistory;

public class GetUserSeasonHistoryRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserSeasonHistoryRequest, IReadOnlyCollection<UserSeasonHistoryDto>>
{
    public async Task<IReadOnlyCollection<UserSeasonHistoryDto>> Handle(GetUserSeasonHistoryRequest request, CancellationToken cancellationToken)
    {
        var standings = await unitOfWork.LeagueSeasonStandingRepository.GetByUserIdWithDetailsAsync(request.UserId);

        return standings
            .Select(lss => new UserSeasonHistoryDto(
                lss.LeagueSeason.Season.SeasonNumber,
                lss.LeagueSeason.Season.SeasonName,
                lss.LeagueSeason.League.LeagueName,
                lss.Place
            ))
            .ToList();
    }
}
