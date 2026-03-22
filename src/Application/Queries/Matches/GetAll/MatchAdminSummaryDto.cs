namespace BldLeague.Application.Queries.Matches.GetAll;

/// <summary>
/// Summary data transfer object for the admin match list, including season, league, round context, and player scores.
/// </summary>
public record MatchAdminSummaryDto(
    Guid Id,
    int SeasonNumber,
    string LeagueIdentifier,
    int RoundNumber,
    string UserAFullName,
    string? UserBFullName,
    int UserAScore,
    int UserBScore);
