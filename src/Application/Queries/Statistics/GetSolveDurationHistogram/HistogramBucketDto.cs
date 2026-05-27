namespace BldLeague.Application.Queries.Statistics.GetSolveDurationHistogram;

/// <summary>
/// Single bucket in a labelled histogram.
/// </summary>
public record HistogramBucketDto(string Label, int Count);
