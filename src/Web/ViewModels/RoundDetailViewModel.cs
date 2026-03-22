using BldLeague.Application.Queries.Rounds.GetDetail;

namespace BldLeague.Web.ViewModels;

public class RoundDetailViewModel
{
    public required string StartDate { get; set; }
    public required string EndDate { get; set; }
    public required List<string> Scrambles { get; set; }
    public required List<RoundStandingViewModel> Standings { get; set; }

    public static RoundDetailViewModel FromDto(RoundDetailDto dto)
    {
        return new RoundDetailViewModel
        {
            StartDate = dto.StartDate.ToShortDateString(),
            EndDate = dto.EndDate.ToShortDateString(),
            Scrambles = dto.Scrambles
                .OrderBy(s => s.ScrambleNumber)
                .Select(s => s.Notation)
                .ToList(),
            Standings = dto.Standings.Select(RoundStandingViewModel.FromDto).ToList()
        };
    }
}