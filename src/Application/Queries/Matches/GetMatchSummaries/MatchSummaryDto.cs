namespace BldLeague.Application.Queries.Matches.GetMatchSummaries;

/// <summary>
/// Summary data transfer object for a match showing the two players and their final scores.
/// </summary>
public class MatchSummaryDto
{
    public Guid Id { get; set; }
    public required string UserAFullName { get; set; }
    public string? UserBFullName { get; set; }
    public int UserAScore { get; set; }
    public int UserBScore { get; set; }
    public DateTime RoundStartDate { get; set; }
    public DateTime RoundEndDate { get; set; }
}
