using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetAccuracyLeaders;

public record GetAccuracyLeadersRequest : IRequest<AccuracyLeadersDto>;
