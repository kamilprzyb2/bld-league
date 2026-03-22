using BldLeague.Application.Queries.Rounds.GetDetail;

namespace BldLeague.Web.ViewModels;

public class RoundStandingViewModel
{
    public required string UserFullName { get; set; }
    public required string LeagueIdentifier { get; set; }
    public required string Solve1 { get; set; }
    public required string Solve2 { get; set; }
    public required string Solve3 { get; set; }
    public required string Solve4 { get; set; }
    public required string Solve5 { get; set; }
    public required string Best { get; set; }
    public required string Average { get; set; }
    public required string Place { get; set; }
    public int Points { get; set; }

    public static RoundStandingViewModel FromDto(RoundStandingDto dto)
    {
        return new RoundStandingViewModel
        {
            UserFullName = dto.UserFullName,
            LeagueIdentifier = dto.LeagueIdentifier,
            Solve1 = dto.Solve1.ToString(),
            Solve2 = dto.Solve2.ToString(),
            Solve3 = dto.Solve3.ToString(),
            Solve4 = dto.Solve4.ToString(),
            Solve5 = dto.Solve5.ToString(),
            Best = dto.Best.ToSummaryString(),
            Average = dto.Average.ToSummaryString(),
            Place = dto.Place.HasValue ? dto.Place.Value.ToString() : "-",
            Points = dto.Points
        };
    }
}
