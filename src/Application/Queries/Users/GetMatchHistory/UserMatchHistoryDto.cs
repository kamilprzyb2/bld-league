namespace BldLeague.Application.Queries.Users.GetMatchHistory;

public record UserMatchHistoryDto(
    Guid MatchId,
    int SeasonNumber,
    int RoundNumber,
    string SeasonName,
    string RoundName,
    Guid SeasonId,
    string LeagueIdentifier,
    string ProfileUserFullName,
    string? OpponentFullName,
    int ProfileUserScore,
    int OpponentScore,
    Guid? OpponentId
);
