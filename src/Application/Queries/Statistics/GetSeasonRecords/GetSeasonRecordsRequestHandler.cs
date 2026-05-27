using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetSeasonRecords;

public class GetSeasonRecordsRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetSeasonRecordsRequest, IReadOnlyList<SeasonRecordDto>>
{
    public async Task<IReadOnlyList<SeasonRecordDto>> Handle(GetSeasonRecordsRequest request, CancellationToken cancellationToken)
        => await unitOfWork.StatisticsRepository.GetSeasonRecordsAsync(roundClock.LocalToday());
}
