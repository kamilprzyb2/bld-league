using BldLeague.Application.Queries.Statistics.GetStatisticsSummary;

namespace BldLeague.Web.ViewModels;

/// <summary>
/// Wraps <see cref="StatisticsSummaryDto"/> with the 3 home-page tiles ready to render.
/// </summary>
public class StatisticsSummaryViewModel
{
    public StatisticsSummaryViewModel(StatisticsSummaryDto summary)
    {
        Summary = summary;
        Tiles =
        [
            new StatTileViewModel(
                IconClass: "bi-grid-3x3-gap-fill",
                IconColorClass: "text-info",
                PrimaryText: $"{summary.ValidSolves} / {summary.Attempts}",
                Subtitle: "Ułożonych kostek"),
            new StatTileViewModel(
                IconClass: "bi-calendar-check",
                IconColorClass: "text-warning",
                PrimaryText: summary.Matches.ToString(),
                Subtitle: "Rozegranych meczów"),
            new StatTileViewModel(
                IconClass: "bi-people-fill",
                IconColorClass: "text-success",
                PrimaryText: summary.Participants.ToString(),
                Subtitle: "Uczestników"),
        ];
    }

    public StatisticsSummaryDto Summary { get; }
    public IReadOnlyList<StatTileViewModel> Tiles { get; }
}
