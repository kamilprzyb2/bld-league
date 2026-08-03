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
    ///
    /// Selection chain: the currently active round if one exists; otherwise the finished round
    /// with the highest RoundNumber; otherwise (no round has finished yet, i.e. the season
    /// hasn't started) the round with the lowest RoundNumber.
    /// </summary>
    public static RoundSummaryDto GetDefaultRound(this IReadOnlyCollection<RoundSummaryDto> rounds, RoundClock clock)
    {
        var active = rounds.FirstOrDefault(r => clock.IsRoundActive(r.StartDate, r.EndDate));
        if (active != null)
            return active;

        var lastFinished = rounds
            .Where(r => clock.IsRoundFinished(r.EndDate))
            .MaxBy(r => r.RoundNumber);

        return lastFinished ?? rounds.MinBy(r => r.RoundNumber)!;
    }
}
