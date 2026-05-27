using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetStreakLeaders;

public class GetStreakLeadersRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetStreakLeadersRequest, StreakLeadersDto>
{
    public async Task<StreakLeadersDto> Handle(GetStreakLeadersRequest request, CancellationToken cancellationToken)
    {
        var localToday = roundClock.LocalToday();

        var solveStreaks = await unitOfWork.StatisticsRepository.GetSolvesGroupedByUserAsync(localToday);
        StreakDto? bestSolveStreak = null;
        foreach (var group in solveStreaks)
        {
            var length = StreakCalculator.LongestSuccessStreak(group.Solves);
            if (length == 0) continue;
            if (bestSolveStreak == null || length > bestSolveStreak.Length)
                bestSolveStreak = new StreakDto(group.UserId, group.FullName, length);
        }

        var winStreaks = await unitOfWork.StatisticsRepository.GetMatchesGroupedByUserAsync(localToday);
        StreakDto? bestWinStreak = null;
        foreach (var group in winStreaks)
        {
            var length = StreakCalculator.LongestWinStreak(group.Matches);
            if (length == 0) continue;
            if (bestWinStreak == null || length > bestWinStreak.Length)
                bestWinStreak = new StreakDto(group.UserId, group.FullName, length);
        }

        return new StreakLeadersDto(bestSolveStreak, bestWinStreak);
    }
}
