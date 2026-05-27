using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetStreakLeaders;

public record GetStreakLeadersRequest : IRequest<StreakLeadersDto>;
