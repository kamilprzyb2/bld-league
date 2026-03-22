namespace BldLeague.Application.Queries.LeagueSeasons.GetAll;

/// <summary>
/// Summary data transfer object representing a league season, including league/season identifiers and roster count.
/// </summary>
public class LeagueSeasonDto
{
    public Guid LeagueSeasonId { get; set; }
    public Guid LeagueId { get; set; }
    public string LeagueIdentifier { get; set; } = string.Empty;
    public string LeagueName { get; set; } = string.Empty;
    public int SeasonNumber { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public Guid SeasonId { get; set; }
}
