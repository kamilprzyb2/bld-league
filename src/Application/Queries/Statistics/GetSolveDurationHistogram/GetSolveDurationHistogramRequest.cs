using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetSolveDurationHistogram;

public record GetSolveDurationHistogramRequest : IRequest<SolveDurationHistogramDto>;
