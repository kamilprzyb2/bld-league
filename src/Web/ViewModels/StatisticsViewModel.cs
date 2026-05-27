using BldLeague.Application.Queries.Statistics.GetAccuracyLeaders;
using BldLeague.Application.Queries.Statistics.GetLeagueRecordsAndAverages;
using BldLeague.Application.Queries.Statistics.GetMatchScoreDistribution;
using BldLeague.Application.Queries.Statistics.GetRollingAo12Leaders;
using BldLeague.Application.Queries.Statistics.GetRollingAo25Leaders;
using BldLeague.Application.Queries.Statistics.GetSeasonRecords;
using BldLeague.Application.Queries.Statistics.GetSolveDurationHistogram;
using BldLeague.Application.Queries.Statistics.GetStreakLeaders;
using BldLeague.Application.Queries.Statistics.GetSubmissionHeatmap;

namespace BldLeague.Web.ViewModels;

/// <summary>
/// Composed view model for the public Statistics page — wraps all sections of data.
/// </summary>
public class StatisticsViewModel
{
    public required StatisticsSummaryViewModel Summary { get; init; }
    public required SubmissionHeatmapDto Heatmap { get; init; }
    public required SolveDurationHistogramDto SolveDurations { get; init; }
    public required ScoreDistributionDto ScoreDistribution { get; init; }
    public required IReadOnlyList<SeasonRecordDto> SeasonRecords { get; init; }
    public required IReadOnlyList<LeagueRecordDto> LeagueRecords { get; init; }
    public required StreakLeadersDto StreakLeaders { get; init; }
    public required AccuracyLeadersDto AccuracyLeaders { get; init; }
    public required RollingAo12LeadersDto RollingAo12Leaders { get; init; }
    public required RollingAo25LeadersDto RollingAo25Leaders { get; init; }
}
