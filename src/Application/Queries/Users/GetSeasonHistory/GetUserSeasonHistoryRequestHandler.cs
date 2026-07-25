using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetSeasonHistory;

public class GetUserSeasonHistoryRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetUserSeasonHistoryRequest, IReadOnlyCollection<UserSeasonHistoryDto>>
{
    public async Task<IReadOnlyCollection<UserSeasonHistoryDto>> Handle(GetUserSeasonHistoryRequest request, CancellationToken cancellationToken)
    {
        var standings = await unitOfWork.LeagueSeasonStandingRepository.GetByUserIdWithDetailsAsync(request.UserId);
        var lastRoundEndDates = await unitOfWork.RoundRepository.GetLastRoundEndDateBySeasonAsync();

        return standings
            .Select(lss => new UserSeasonHistoryDto(
                lss.LeagueSeason.Season.SeasonNumber,
                lss.LeagueSeason.Season.SeasonName,
                lss.LeagueSeason.League.LeagueName,
                lss.Place,
                lastRoundEndDates.TryGetValue(lss.LeagueSeason.SeasonId, out var lastEndDate)
                    && roundClock.IsRoundFinished(lastEndDate),
                lss.MatchesPlayed
            ))
            .ToList();
    }
}
