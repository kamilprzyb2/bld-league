namespace BldLeague.Application.Queries.Users.GetByLeagueSeasonId;

/// <summary>
/// Summary data for a user enrolled in a specific league season, including their subleague group.
/// </summary>
public record LeagueSeasonUserDto(Guid Id, string FullName, string WcaId, int SubleagueGroup);
