namespace BldLeague.Application.Queries.LeagueSeasons.GetDetail;

/// <summary>
/// Detailed data transfer object for a league season, containing the ordered list of player standings.
/// </summary>
public class LeagueSeasonDetailDto
{
    public required List<LeagueSeasonStandingDto> Standings { get; set; }
}
