namespace BldLeague.Application.Queries.Users.GetSeasonHistory;

public record UserSeasonHistoryDto(
    int SeasonNumber,
    string SeasonName,
    string LeagueName,
    int Place
);
