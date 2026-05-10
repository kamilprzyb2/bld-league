using MediatR;

namespace BldLeague.Application.Queries.Matches.GetRecentFinishedMatches;

public record GetRecentFinishedMatchesRequest(int Count) : IRequest<IReadOnlyList<RecentMatchDto>>;
