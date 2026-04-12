using BldLeague.Application.Abstractions.Repositories;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetRoundResults;

public class GetUserRoundResultsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserRoundResultsRequest, IReadOnlyCollection<UserRoundResultDto>>
{
    public async Task<IReadOnlyCollection<UserRoundResultDto>> Handle(GetUserRoundResultsRequest request, CancellationToken cancellationToken)
    {
        var standings = await unitOfWork.RoundStandingRepository.GetByUserIdWithDetailsAsync(request.UserId);

        return standings
            .Select(rs => new UserRoundResultDto(
                rs.Round.Season.SeasonNumber,
                rs.Round.RoundNumber,
                rs.Round.Season.SeasonName,
                rs.Round.RoundName,
                rs.League.LeagueIdentifier,
                rs.Round.SeasonId,
                rs.Place,
                rs.Best,
                rs.Average,
                rs.Solve1,
                rs.Solve2,
                rs.Solve3,
                rs.Solve4,
                rs.Solve5
            ))
            .ToList();
    }
}
