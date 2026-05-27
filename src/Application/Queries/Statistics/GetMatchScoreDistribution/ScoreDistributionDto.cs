using BldLeague.Application.Queries.Statistics.GetSolveDurationHistogram;

namespace BldLeague.Application.Queries.Statistics.GetMatchScoreDistribution;

/// <summary>
/// Distribution of (winner, loser) score pairs across finished matches vs a real opponent.
/// BYE matches are excluded. Each match contributes one entry; total per match is 6 points.
/// </summary>
public record ScoreDistributionDto(IReadOnlyList<HistogramBucketDto> Buckets);
