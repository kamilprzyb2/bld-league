using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetStatisticsSummary;

public record GetStatisticsSummaryRequest : IRequest<StatisticsSummaryDto>;
