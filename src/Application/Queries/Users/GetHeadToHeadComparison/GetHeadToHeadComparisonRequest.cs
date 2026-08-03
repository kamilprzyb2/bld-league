using MediatR;

namespace BldLeague.Application.Queries.Users.GetHeadToHeadComparison;

public record GetHeadToHeadComparisonRequest(Guid UserAId, Guid UserBId) : IRequest<HeadToHeadComparisonDto?>;
