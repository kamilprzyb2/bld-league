namespace BldLeague.Application.Queries.Matches.GetRecentFinishedMatches;

public class RecentMatchDto
{
    public Guid MatchId { get; init; }
    public required string UserAFullName { get; init; }
    public string? UserBFullName { get; init; }
    public int UserAScore { get; init; }
    public int UserBScore { get; init; }
    public required string LeagueIdentifier { get; init; }
    public int RoundNumber { get; init; }
    public bool IsFromActiveRound { get; init; }
}
