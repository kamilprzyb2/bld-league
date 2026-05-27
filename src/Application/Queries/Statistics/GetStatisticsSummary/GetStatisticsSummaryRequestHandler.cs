using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetStatisticsSummary;

public class GetStatisticsSummaryRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetStatisticsSummaryRequest, StatisticsSummaryDto>
{
    public async Task<StatisticsSummaryDto> Handle(GetStatisticsSummaryRequest request, CancellationToken cancellationToken)
        => await unitOfWork.StatisticsRepository.GetSummaryAsync(roundClock.LocalToday());
}
