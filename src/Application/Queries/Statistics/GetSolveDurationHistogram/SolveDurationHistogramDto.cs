namespace BldLeague.Application.Queries.Statistics.GetSolveDurationHistogram;

/// <summary>
/// Solve duration distribution: 19 buckets — <c>&lt;10s</c>, <c>10–20s</c>, …, <c>2:50–3:00</c>, <c>3:00+</c>.
/// </summary>
public record SolveDurationHistogramDto(IReadOnlyList<HistogramBucketDto> Buckets);
