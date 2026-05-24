using BldLeague.Application.Queries.Statistics.GetLeagueRecordsAndAverages;
using BldLeague.Application.Queries.Statistics.GetMatchScoreDistribution;
using BldLeague.Application.Queries.Statistics.GetSeasonRecords;
using BldLeague.Application.Queries.Statistics.GetSolveDurationHistogram;
using BldLeague.Application.Queries.Statistics.GetStreakLeaders;
using BldLeague.Application.Queries.Statistics.GetSubmissionHeatmap;
using BldLeague.Application.Queries.Statistics.GetSubmissionHourHistogram;

namespace BldLeague.Web.ViewModels;

/// <summary>
/// Composed view model for the public Statistics page — wraps all eight sections of data.
/// </summary>
public class StatisticsViewModel
{
    public required StatisticsSummaryViewModel Summary { get; init; }
    public required SubmissionHourHistogramDto HourHistogram { get; init; }
    public required SubmissionHeatmapDto Heatmap { get; init; }
    public required SolveDurationHistogramDto SolveDurations { get; init; }
    public required ScoreDistributionDto ScoreDistribution { get; init; }
    public required IReadOnlyList<SeasonRecordDto> SeasonRecords { get; init; }
    public required IReadOnlyList<LeagueRecordDto> LeagueRecords { get; init; }
    public required StreakLeadersDto StreakLeaders { get; init; }
}
