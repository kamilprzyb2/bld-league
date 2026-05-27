using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetRecentFinishedMatches;

public class GetRecentFinishedMatchesRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetRecentFinishedMatchesRequest, IReadOnlyList<RecentMatchDto>>
{
    public async Task<IReadOnlyList<RecentMatchDto>> Handle(GetRecentFinishedMatchesRequest request, CancellationToken cancellationToken)
        => await unitOfWork.MatchRepository.GetRecentFinishedMatchesAsync(request.Count, roundClock.LocalToday());
}
