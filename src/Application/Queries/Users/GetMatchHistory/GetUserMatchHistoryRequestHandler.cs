using BldLeague.Application.Abstractions.Repositories;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetMatchHistory;

public class GetUserMatchHistoryRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserMatchHistoryRequest, IReadOnlyCollection<UserMatchHistoryDto>>
{
    public async Task<IReadOnlyCollection<UserMatchHistoryDto>> Handle(GetUserMatchHistoryRequest request, CancellationToken cancellationToken)
    {
        var matches = await unitOfWork.MatchRepository.GetFinishedMatchesByUserIdAsync(request.UserId);

        return matches
            .Select(m =>
            {
                bool isUserA = m.UserAId == request.UserId;
                string profileUserFullName = isUserA ? m.UserA.FullName : m.UserB!.FullName;
                string? opponentFullName = isUserA ? m.UserB?.FullName : m.UserA.FullName;
                int profileUserScore = isUserA ? m.UserAScore : m.UserBScore;
                int opponentScore = isUserA ? m.UserBScore : m.UserAScore;
                Guid? opponentId = isUserA ? m.UserBId : m.UserAId;

                return new UserMatchHistoryDto(
                    m.Id,
                    m.Round.Season.SeasonNumber,
                    m.Round.RoundNumber,
                    m.Round.Season.SeasonName,
                    m.Round.RoundName,
                    m.Round.SeasonId,
                    m.LeagueSeason.League.LeagueIdentifier,
                    profileUserFullName,
                    opponentFullName,
                    profileUserScore,
                    opponentScore,
                    opponentId
                );
            })
            .ToList();
    }
}
