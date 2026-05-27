using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetLeagueRecordsAndAverages;

public class GetLeagueRecordsAndAveragesRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetLeagueRecordsAndAveragesRequest, IReadOnlyList<LeagueRecordDto>>
{
    public async Task<IReadOnlyList<LeagueRecordDto>> Handle(GetLeagueRecordsAndAveragesRequest request, CancellationToken cancellationToken)
        => await unitOfWork.StatisticsRepository.GetLeagueRecordsAndAveragesAsync(roundClock.LocalToday());
}
