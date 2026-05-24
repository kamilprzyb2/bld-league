using BldLeague.Application.Queries.Statistics.GetLeagueRecordsAndAverages;
using BldLeague.Application.Queries.Statistics.GetMatchScoreDistribution;
using BldLeague.Application.Queries.Statistics.GetSeasonRecords;
using BldLeague.Application.Queries.Statistics.GetSolveDurationHistogram;
using BldLeague.Application.Queries.Statistics.GetStatisticsSummary;
using BldLeague.Application.Queries.Statistics.GetStreakLeaders;
using BldLeague.Application.Queries.Statistics.GetSubmissionHeatmap;
using BldLeague.Application.Queries.Statistics.GetSubmissionHourHistogram;
using BldLeague.Web.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BldLeague.Web.Pages.Statistics;

public class Statistics(IMediator mediator) : PageModel
{
    public StatisticsViewModel? Data { get; set; }

    public async Task OnGet()
    {
        // Queries are awaited sequentially — the shared single-DbContext UnitOfWork
        // is not concurrency-safe, so we never fan out with Task.WhenAll.
        var summary = await mediator.Send(new GetStatisticsSummaryRequest());
        var hourHistogram = await mediator.Send(new GetSubmissionHourHistogramRequest());
        var heatmap = await mediator.Send(new GetSubmissionHeatmapRequest());
        var durations = await mediator.Send(new GetSolveDurationHistogramRequest());
        var scores = await mediator.Send(new GetMatchScoreDistributionRequest());
        var seasonRecords = await mediator.Send(new GetSeasonRecordsRequest());
        var leagueRecords = await mediator.Send(new GetLeagueRecordsAndAveragesRequest());
        var streaks = await mediator.Send(new GetStreakLeadersRequest());

        Data = new StatisticsViewModel
        {
            Summary = new StatisticsSummaryViewModel(summary),
            HourHistogram = hourHistogram,
            Heatmap = heatmap,
            SolveDurations = durations,
            ScoreDistribution = scores,
            SeasonRecords = seasonRecords,
            LeagueRecords = leagueRecords,
            StreakLeaders = streaks,
        };
    }

    public static string[] PolishDayNames() => ["Pon", "Wt", "Śr", "Czw", "Pt", "Sob", "Nie"];
}
