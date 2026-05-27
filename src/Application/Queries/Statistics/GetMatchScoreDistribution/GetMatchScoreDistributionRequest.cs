using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetMatchScoreDistribution;

public record GetMatchScoreDistributionRequest : IRequest<ScoreDistributionDto>;
