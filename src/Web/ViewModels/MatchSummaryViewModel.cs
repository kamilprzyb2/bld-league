using BldLeague.Application.Queries.Matches.GetMatchSummaries;

namespace BldLeague.Web.ViewModels;

public class MatchSummaryViewModel
{
    public Guid MatchId { get; set; }
    public required string UserAFullName { get; set; }
    public string? UserBFullName { get; set; }
    public int UserAScore { get; set; }
    public int UserBScore { get; set; }
    public MatchStatus Status { get; set; }

    protected MatchSummaryViewModel() {}

    public static MatchSummaryViewModel FromDto(MatchSummaryDto dto)
    {
        var now = DateTime.UtcNow;
        var status = dto.RoundEndDate < now
            ? MatchStatus.Finished
            : dto.RoundStartDate <= now
                ? MatchStatus.InProgress
                : MatchStatus.Upcoming;

        return new MatchSummaryViewModel
        {
            MatchId = dto.Id,
            UserAFullName = dto.UserAFullName,
            UserBFullName = dto.UserBFullName,
            UserAScore = dto.UserAScore,
            UserBScore = dto.UserBScore,
            Status = status,
        };
    }
}
