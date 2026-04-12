using BldLeague.Application.Queries.LeagueSeasons.GetDetail;

namespace BldLeague.Web.ViewModels;

public class LeagueSeasonDetailViewModel
{
    public required List<LeagueSeasonStandingViewModel> Standings { get; set; }
    public int PromotionCount { get; set; }
    public int RelegationCount { get; set; }
    public int PlayoffPromotionCount { get; set; }
    public int PlayoffRelegationCount { get; set; }

    public static LeagueSeasonDetailViewModel FromDto(LeagueSeasonDetailDto dto)
    {
        return new LeagueSeasonDetailViewModel
        {
            Standings = dto.Standings.Select(LeagueSeasonStandingViewModel.FromDto).ToList(),
            PromotionCount = dto.PromotionCount,
            RelegationCount = dto.RelegationCount,
            PlayoffPromotionCount = dto.PlayoffPromotionCount,
            PlayoffRelegationCount = dto.PlayoffRelegationCount,
        };
    }
}