using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetRecentFinishedMatches;

public class GetRecentFinishedMatchesRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetRecentFinishedMatchesRequest, IReadOnlyList<RecentMatchDto>>
{
    public async Task<IReadOnlyList<RecentMatchDto>> Handle(GetRecentFinishedMatchesRequest request, CancellationToken cancellationToken)
    {
        var localToday = roundClock.LocalToday();
        var projections = await unitOfWork.MatchRepository.GetRecentFinishedMatchesAsync(request.Count, localToday);

        return projections
            .Select(p => new RecentMatchDto
            {
                MatchId = p.MatchId,
                UserAFullName = p.UserAFullName,
                UserBFullName = p.UserBFullName,
                UserAScore = p.UserAScore,
                UserBScore = p.UserBScore,
                LeagueIdentifier = p.LeagueIdentifier,
                SeasonNumber = p.SeasonNumber,
                RoundNumber = p.RoundNumber,
                IsFromActiveRound = roundClock.IsRoundActive(p.RoundStartDate, p.RoundEndDate),
            })
            .ToList();
    }
}
