using BldLeague.Application.Queries.Rounds.GetActiveRoundLiveDetail;

namespace BldLeague.Web.ViewModels;

/// <summary>
/// Renders a single row in the active-round live view. Solve / Best / Average strings are populated only
/// for rows in the "Wyniki" (finished) section; the other two sections use the identity factory which
/// leaves them empty so the view can render a dash.
/// </summary>
public class ActiveRoundLiveRowViewModel
{
    public Guid UserId { get; set; }
    public required string UserFullName { get; set; }
    public required string LeagueIdentifier { get; set; }
    public string Solve1 { get; set; } = string.Empty;
    public string Solve2 { get; set; } = string.Empty;
    public string Solve3 { get; set; } = string.Empty;
    public string Solve4 { get; set; } = string.Empty;
    public string Solve5 { get; set; } = string.Empty;
    public string Best { get; set; } = string.Empty;
    public string Average { get; set; } = string.Empty;

    public static ActiveRoundLiveRowViewModel FromFinishedDto(LiveRoundRowDto dto)
    {
        return new ActiveRoundLiveRowViewModel
        {
            UserId = dto.UserId,
            UserFullName = dto.UserFullName,
            LeagueIdentifier = dto.LeagueIdentifier,
            Solve1 = dto.Solve1.ToString(),
            Solve2 = dto.Solve2.ToString(),
            Solve3 = dto.Solve3.ToString(),
            Solve4 = dto.Solve4.ToString(),
            Solve5 = dto.Solve5.ToString(),
            Best = dto.Best.ToSummaryString(),
            Average = dto.Average.ToSummaryString(),
        };
    }

    public static ActiveRoundLiveRowViewModel FromIdentityDto(LiveRoundRowDto dto)
    {
        return new ActiveRoundLiveRowViewModel
        {
            UserId = dto.UserId,
            UserFullName = dto.UserFullName,
            LeagueIdentifier = dto.LeagueIdentifier,
        };
    }
}
