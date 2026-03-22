using BldLeague.Application.Queries.LeagueSeasons.GetDetail;

namespace BldLeague.Web.ViewModels;

public class LeagueSeasonDetailViewModel
{
    public required List<LeagueSeasonStandingViewModel> Standings { get; set; }

    public static LeagueSeasonDetailViewModel FromDto(LeagueSeasonDetailDto dto)
    {
        return new LeagueSeasonDetailViewModel
        {
            Standings = dto.Standings.Select(LeagueSeasonStandingViewModel.FromDto).ToList(),
        };
    }
}