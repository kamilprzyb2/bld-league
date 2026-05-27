using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Scoring;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetRollingAo25Leaders;

public class GetRollingAo25LeadersRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetRollingAo25LeadersRequest, RollingAo25LeadersDto>
{
    private const int WindowSize = 25;

    public async Task<RollingAo25LeadersDto> Handle(GetRollingAo25LeadersRequest request, CancellationToken cancellationToken)
    {
        var localToday = roundClock.LocalToday();
        var groups = await unitOfWork.StatisticsRepository.GetNonDnsSolvesGroupedByUserAsync(localToday);

        var entries = new List<(Guid UserId, string FullName, SolveResult Best, IReadOnlyList<SolveResult> Window, int TotalSolves)>();

        foreach (var group in groups)
        {
            if (group.Solves.Count < WindowSize) continue;

            SolveResult? bestAo25 = null;
            IReadOnlyList<SolveResult>? bestWindow = null;

            for (int start = 0; start + WindowSize <= group.Solves.Count; start++)
            {
                var window = new List<SolveResult>(WindowSize);
                for (int i = 0; i < WindowSize; i++)
                    window.Add(group.Solves[start + i]);

                var ao25 = AverageCalculator.CalculateAo25(window);
                if (!ao25.IsValid) continue;

                if (bestAo25 == null || ao25.Centiseconds < bestAo25.Value.Centiseconds)
                {
                    bestAo25 = ao25;
                    bestWindow = window;
                }
            }

            if (bestAo25 == null || bestWindow == null) continue;

            entries.Add((group.UserId, group.FullName, bestAo25.Value, bestWindow, group.Solves.Count));
        }

        // Return every qualifier; the view decides how many to surface.
        var sorted = entries
            .OrderBy(e => e.Best.Centiseconds)
            .ThenByDescending(e => e.TotalSolves)
            .ThenBy(e => e.UserId)
            .Select(e => new RollingAo25EntryDto(e.UserId, e.FullName, e.Best, e.Window))
            .ToList();

        return new RollingAo25LeadersDto(sorted);
    }
}
