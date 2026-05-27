using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Queries.Statistics.GetSolveDurationHistogram;

public class GetSolveDurationHistogramRequestHandler(IUnitOfWork unitOfWork, RoundClock roundClock)
    : IRequestHandler<GetSolveDurationHistogramRequest, SolveDurationHistogramDto>
{
    // 19 buckets: <10s, 10–20s, 20–30s, …, 2:50–3:00, 3:00+
    // Buckets are 10s wide up to 180s (= 3:00).
    private const int BucketWidthCs = 1000; // 10 seconds in centiseconds
    private const int BucketCount = 19;
    private const int OverflowThresholdCs = 18 * BucketWidthCs; // 180s = 3:00

    public async Task<SolveDurationHistogramDto> Handle(GetSolveDurationHistogramRequest request, CancellationToken cancellationToken)
    {
        var localToday = roundClock.LocalToday();
        var solveCentiseconds = await unitOfWork.StatisticsRepository.GetValidSolveCentisecondsAsync(localToday);

        var counts = new int[BucketCount];
        foreach (var cs in solveCentiseconds)
        {
            int index;
            if (cs >= OverflowThresholdCs)
                index = BucketCount - 1;
            else
                index = cs / BucketWidthCs;
            counts[index]++;
        }

        var labels = BuildLabels();
        var buckets = new HistogramBucketDto[BucketCount];
        for (int i = 0; i < BucketCount; i++)
            buckets[i] = new HistogramBucketDto(labels[i], counts[i]);

        return new SolveDurationHistogramDto(buckets);
    }

    private static string[] BuildLabels()
    {
        var labels = new string[BucketCount];
        labels[0] = "<10s";
        for (int i = 1; i < BucketCount - 1; i++)
        {
            int lowSeconds = i * 10;
            int highSeconds = (i + 1) * 10;
            // Use raw "Ns" suffix only when both ends are <60s, else use m:ss form for both.
            if (highSeconds <= 60)
                labels[i] = $"{lowSeconds}–{highSeconds}s";
            else
                labels[i] = $"{FormatClock(lowSeconds)}–{FormatClock(highSeconds)}";
        }
        labels[BucketCount - 1] = "3:00+";
        return labels;
    }

    private static string FormatClock(int seconds)
    {
        int minutes = seconds / 60;
        int sec = seconds % 60;
        return $"{minutes}:{sec:00}";
    }
}
