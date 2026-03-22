using BldLeague.Application.Queries.Matches.GetMatchSummaries;

namespace BldLeague.Web.ViewModels;

public class MatchSummaryViewModel
{
    public Guid MatchId { get; set; }
    public required string UserAFullName { get; set; }
    public string? UserBFullName { get; set; }
    public int UserAScore { get; set; }
    public int UserBScore { get; set; }

    protected MatchSummaryViewModel() {}

    public static MatchSummaryViewModel FromDto(MatchSummaryDto dto)
    {
        return new MatchSummaryViewModel
        {
            MatchId = dto.Id,
            UserAFullName = dto.UserAFullName,
            UserBFullName = dto.UserBFullName,
            UserAScore = dto.UserAScore,
            UserBScore = dto.UserBScore,
        };
    }
}
