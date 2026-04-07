using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.LeagueSeasons.GetDetail;

/// <summary>
/// Data transfer object representing a single player's cumulative standing within a league season.
/// </summary>
public class LeagueSeasonStandingDto
{
    public required string UserFullName { get; set; }
    public int Place { get; set; }
    public int SubleagueGroup { get; set; }
    public int MatchesPlayed { get; set; }
    public int MatchesWon { get; set; }
    public int MatchesTied { get; set; }
    public int MatchesLost { get; set; }
    public int BigPoints { get; set; }
    public int BonusPoints { get; set; }
    public int SmallPointsGained { get; set; }
    public int SmallPointsLost { get; set; }
    public int SmallPointsBalance { get; set; }
    public SolveResult Best { get; set; }
}
