using BldLeague.Application.Queries.Statistics.GetAccuracyLeaders;
using BldLeague.Application.Queries.Statistics.GetLeagueRecordsAndAverages;
using BldLeague.Application.Queries.Statistics.GetMatchScoreDistribution;
using BldLeague.Application.Queries.Statistics.GetRollingAo12Leaders;
using BldLeague.Application.Queries.Statistics.GetRollingAo25Leaders;
using BldLeague.Application.Queries.Statistics.GetSeasonRecords;
using BldLeague.Application.Queries.Statistics.GetSolveDurationHistogram;
using BldLeague.Application.Queries.Statistics.GetStatisticsSummary;
using BldLeague.Application.Queries.Statistics.GetStreakLeaders;
using BldLeague.Application.Queries.Statistics.GetSubmissionHeatmap;
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
        var heatmap = await mediator.Send(new GetSubmissionHeatmapRequest());
        var durations = await mediator.Send(new GetSolveDurationHistogramRequest());
        var scores = await mediator.Send(new GetMatchScoreDistributionRequest());
        var seasonRecords = await mediator.Send(new GetSeasonRecordsRequest());
        var leagueRecords = await mediator.Send(new GetLeagueRecordsAndAveragesRequest());
        var streaks = await mediator.Send(new GetStreakLeadersRequest());
        var accuracy = await mediator.Send(new GetAccuracyLeadersRequest());
        var rollingAo12 = await mediator.Send(new GetRollingAo12LeadersRequest());
        var rollingAo25 = await mediator.Send(new GetRollingAo25LeadersRequest());

        Data = new StatisticsViewModel
        {
            Summary = new StatisticsSummaryViewModel(summary),
            Heatmap = heatmap,
            SolveDurations = durations,
            ScoreDistribution = scores,
            SeasonRecords = seasonRecords,
            LeagueRecords = leagueRecords,
            StreakLeaders = streaks,
            AccuracyLeaders = accuracy,
            RollingAo12Leaders = rollingAo12,
            RollingAo25Leaders = rollingAo25,
        };
    }

    public static string[] PolishDayNames() => ["Pon", "Wt", "Śr", "Czw", "Pt", "Sob", "Nie"];
}
