namespace BldLeague.Application.Queries.Users.GetHeadToHeadComparison;

public record HeadToHeadComparisonDto(
    PlayerComparisonSideDto PlayerA,
    PlayerComparisonSideDto PlayerB,
    int WinsA,
    int WinsB,
    int Draws,
    int SolvePointsA,
    int SolvePointsB,
    IReadOnlyList<HeadToHeadMatchDto> Meetings,
    UpcomingMeetingDto? UpcomingMeeting,
    IReadOnlyList<CommonRoundDto> CommonRounds,
    int CommonRoundWinsA,
    int CommonRoundWinsB,
    int CommonRoundTies
);
