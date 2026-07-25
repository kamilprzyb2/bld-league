using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetMatchHistory;

public class GetUserMatchHistoryRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetUserMatchHistoryRequest, IReadOnlyCollection<UserMatchHistoryDto>>
{
    public async Task<IReadOnlyCollection<UserMatchHistoryDto>> Handle(GetUserMatchHistoryRequest request, CancellationToken cancellationToken)
    {
        var matches = await unitOfWork.MatchRepository.GetFinishedMatchesByUserIdAsync(request.UserId, roundClock.LocalToday());

        return matches
            .Select(m =>
            {
                var perspective = MatchPerspective.For(m, request.UserId);

                return new UserMatchHistoryDto(
                    m.Id,
                    m.Round.Season.SeasonNumber,
                    m.Round.RoundNumber,
                    m.Round.Season.SeasonName,
                    m.Round.RoundName,
                    m.Round.SeasonId,
                    m.LeagueSeason.League.LeagueIdentifier,
                    perspective.SelfFullName,
                    perspective.OpponentFullName,
                    perspective.SelfScore,
                    perspective.OpponentScore,
                    perspective.OpponentId
                );
            })
            .ToList();
    }
}
