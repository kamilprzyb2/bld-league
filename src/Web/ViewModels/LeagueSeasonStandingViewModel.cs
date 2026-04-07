using BldLeague.Application.Queries.LeagueSeasons.GetDetail;

namespace BldLeague.Web.ViewModels;

public class LeagueSeasonStandingViewModel
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
    public required string Best { get; set; }

    public static LeagueSeasonStandingViewModel FromDto(LeagueSeasonStandingDto dto)
    {
        return new LeagueSeasonStandingViewModel
        {
            UserFullName = dto.UserFullName,
            Place = dto.Place,
            SubleagueGroup = dto.SubleagueGroup,
            MatchesPlayed = dto.MatchesPlayed,
            MatchesWon = dto.MatchesWon,
            MatchesTied = dto.MatchesTied,
            MatchesLost = dto.MatchesLost,
            BigPoints = dto.BigPoints,
            BonusPoints = dto.BonusPoints,
            SmallPointsGained = dto.SmallPointsGained,
            SmallPointsLost = dto.SmallPointsLost,
            SmallPointsBalance = dto.SmallPointsBalance,
            Best = dto.Best.ToString()
        };
    }
}
