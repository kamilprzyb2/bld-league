namespace BldLeague.Application.Queries.Users.GetHeadToHeadComparison;

public record HeadToHeadMatchDto(
    Guid MatchId,
    int SeasonNumber,
    int RoundNumber,
    string LeagueIdentifier,
    int ScoreA,
    int ScoreB
);
