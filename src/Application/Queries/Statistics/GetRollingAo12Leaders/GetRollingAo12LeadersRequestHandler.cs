using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Scoring;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetRollingAo12Leaders;

public class GetRollingAo12LeadersRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetRollingAo12LeadersRequest, RollingAo12LeadersDto>
{
    private const int WindowSize = 12;

    public async Task<RollingAo12LeadersDto> Handle(GetRollingAo12LeadersRequest request, CancellationToken cancellationToken)
    {
        var localToday = roundClock.LocalToday();
        var groups = await unitOfWork.StatisticsRepository.GetNonDnsSolvesGroupedByUserAsync(localToday);

        var entries = new List<(Guid UserId, string FullName, SolveResult Best, IReadOnlyList<SolveResult> Window, int TotalSolves)>();

        foreach (var group in groups)
        {
            if (group.Solves.Count < WindowSize) continue;

            SolveResult? bestAo12 = null;
            IReadOnlyList<SolveResult>? bestWindow = null;

            for (int start = 0; start + WindowSize <= group.Solves.Count; start++)
            {
                var window = new List<SolveResult>(WindowSize);
                for (int i = 0; i < WindowSize; i++)
                    window.Add(group.Solves[start + i]);

                var ao12 = AverageCalculator.CalculateAo12(window);
                if (!ao12.IsValid) continue;

                if (bestAo12 == null || ao12.Centiseconds < bestAo12.Value.Centiseconds)
                {
                    bestAo12 = ao12;
                    bestWindow = window;
                }
            }

            if (bestAo12 == null || bestWindow == null) continue;

            entries.Add((group.UserId, group.FullName, bestAo12.Value, bestWindow, group.Solves.Count));
        }

        // Return every qualifier; the view decides how many to surface.
        var sorted = entries
            .OrderBy(e => e.Best.Centiseconds)
            .ThenByDescending(e => e.TotalSolves)
            .ThenBy(e => e.UserId)
            .Select(e => new RollingAo12EntryDto(e.UserId, e.FullName, e.Best, e.Window))
            .ToList();

        return new RollingAo12LeadersDto(sorted);
    }
}
