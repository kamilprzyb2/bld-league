namespace BldLeague.Application.Queries.Rounds.GetAllBySeasonId;

/// <summary>
/// Summary data transfer object for a round, used in the public-facing round selector and standings pages.
/// </summary>
public record RoundSummaryDto(Guid Id, Guid SeasonId, int RoundNumber, DateTime StartDate, DateTime EndDate)
{
    public string RoundName => $"Kolejka {RoundNumber}";
}

/// <summary>
/// Extension methods for collections of <see cref="RoundSummaryDto"/> providing default round selection logic.
/// </summary>
public static class RoundSummaryDtoExtensions
{
    /// <summary>
    /// Returns the default round to display when the user hasn't picked one.
    /// Assumes <paramref name="rounds"/> is ordered by RoundNumber ascending (as returned by the repository).
    ///
    /// Current behaviour: show the round immediately before the active one, so the page
    /// opens on the most recently completed round that already has results.
    /// If the active round is the first one (no previous), or if no round is active,
    /// falls back to the round with the highest RoundNumber.
    ///
    /// When live results are available, swap to the commented-out line below to open
    /// directly on the in-progress round instead.
    /// </summary>
    public static RoundSummaryDto GetDefaultRound(this IReadOnlyCollection<RoundSummaryDto> rounds, DateTime today)
    {
        // Future: open directly on the active (in-progress) round.
        // var active = rounds.FirstOrDefault(r => r.StartDate <= today && today <= r.EndDate);
        // return active ?? rounds.Last();

        var list = rounds.ToList(); // already sorted ascending by RoundNumber
        var activeIndex = list.FindIndex(r => r.StartDate <= today && today <= r.EndDate);

        // One round before the active one; fall back to the last round if there is no
        // active round or if the active round is the very first one.
        return activeIndex > 0 ? list[activeIndex - 1] : list[^1];
    }
}
