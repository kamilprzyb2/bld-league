namespace BldLeague.Application.Queries.Matches.GetRecentFinishedMatches;

/// <summary>
/// Repository-level projection of a recent finished match. Carries the round date window
/// so the handler can compute <see cref="RecentMatchDto.IsFromActiveRound"/> via <see cref="Common.RoundClock"/>.
/// </summary>
public class RecentMatchProjection
{
    public Guid MatchId { get; init; }
    public required string UserAFullName { get; init; }
    public string? UserBFullName { get; init; }
    public int UserAScore { get; init; }
    public int UserBScore { get; init; }
    public required string LeagueIdentifier { get; init; }
    public int SeasonNumber { get; init; }
    public int RoundNumber { get; init; }
    public DateTime RoundStartDate { get; init; }
    public DateTime RoundEndDate { get; init; }
}
