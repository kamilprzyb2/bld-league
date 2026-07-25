namespace BldLeague.Application.Queries.Users.GetHeadToHeadComparison;

/// <summary>
/// The unfinished match between the two compared players, if any.
/// Intentionally carries no scores or solves — the round may still be in progress.
/// </summary>
public record UpcomingMeetingDto(
    int SeasonNumber,
    int RoundNumber,
    string LeagueIdentifier,
    bool IsInProgress
);
