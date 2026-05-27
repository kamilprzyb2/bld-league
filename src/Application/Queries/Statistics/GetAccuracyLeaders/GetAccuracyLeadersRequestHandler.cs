using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetAccuracyLeaders;

public class GetAccuracyLeadersRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetAccuracyLeadersRequest, AccuracyLeadersDto>
{
    private const int MinAttempts = 10;

    public async Task<AccuracyLeadersDto> Handle(GetAccuracyLeadersRequest request, CancellationToken cancellationToken)
    {
        var entries = await unitOfWork.StatisticsRepository.GetAccuracyLeadersAsync(
            roundClock.LocalToday(), MinAttempts);
        return new AccuracyLeadersDto(entries);
    }
}
