using BldLeague.Application.Common;

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
    /// Returns the currently active round if one exists; otherwise falls back to the round with the highest RoundNumber.
    /// </summary>
    public static RoundSummaryDto GetDefaultRound(this IReadOnlyCollection<RoundSummaryDto> rounds, RoundClock clock)
    {
        var active = rounds.FirstOrDefault(r => clock.IsRoundActive(r.StartDate, r.EndDate));
        return active ?? rounds.Last();
    }
}
