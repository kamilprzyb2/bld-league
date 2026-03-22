using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Matches.GetMatchSummaries;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetMatchSummaries;

/// <summary>
/// Handles retrieving match summaries applying the optional season, league, and round filters.
/// </summary>
public class GetMatchSummariesRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetMatchSummariesRequest, IReadOnlyCollection<MatchSummaryDto>>
{
    public async Task<IReadOnlyCollection<MatchSummaryDto>> Handle(GetMatchSummariesRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.MatchRepository.GetMatchSummaries(request.SeasonId, request.LeagueId, request.RoundNumber);
    }
}
