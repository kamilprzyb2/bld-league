using BldLeague.Application.Queries.Matches.GetMatchSummaries;

namespace BldLeague.Web.ViewModels;

public class MatchSummaryViewModel
{
    public Guid MatchId { get; set; }
    public Guid UserAId { get; set; }
    public Guid? UserBId { get; set; }
    public required string UserAFullName { get; set; }
    public string? UserBFullName { get; set; }
    public int UserAScore { get; set; }
    public int UserBScore { get; set; }
    public MatchStatus Status { get; set; }

    protected MatchSummaryViewModel() {}

    public static MatchSummaryViewModel FromDto(MatchSummaryDto dto)
    {
        var now = DateTime.UtcNow;
        MatchStatus status;
        if (now > dto.RoundEndDate || dto.BothSidesSubmitted)
            status = MatchStatus.Finished;
        else if (now >= dto.RoundStartDate)
            status = MatchStatus.InProgress;
        else
            status = MatchStatus.Upcoming;

        return new MatchSummaryViewModel
        {
            MatchId = dto.Id,
            UserAId = dto.UserAId,
            UserBId = dto.UserBId,
            UserAFullName = dto.UserAFullName,
            UserBFullName = dto.UserBFullName,
            UserAScore = dto.UserAScore,
            UserBScore = dto.UserBScore,
            Status = status,
        };
    }
}
