using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetSubmissionHeatmap;

public class GetSubmissionHeatmapRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetSubmissionHeatmapRequest, SubmissionHeatmapDto>
{
    public async Task<SubmissionHeatmapDto> Handle(GetSubmissionHeatmapRequest request, CancellationToken cancellationToken)
    {
        var localToday = roundClock.LocalToday();
        var rawSubmissions = await unitOfWork.StatisticsRepository.GetSubmissionTimestampsAsync(localToday);

        var matrix = new int[7, 24];
        int max = 0;
        foreach (var utc in rawSubmissions.Timestamps)
        {
            var local = roundClock.ToLocal(utc);
            // Convert DayOfWeek (Sunday=0) to Polish week (Monday=0)
            int day = ((int)local.DayOfWeek + 6) % 7;
            int hour = local.Hour;
            matrix[day, hour]++;
            if (matrix[day, hour] > max) max = matrix[day, hour];
        }

        return new SubmissionHeatmapDto(matrix, max, rawSubmissions.IncludedMatches, rawSubmissions.TotalMatches);
    }
}
