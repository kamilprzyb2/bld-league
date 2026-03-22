namespace BldLeague.Application.Queries.Leagues.GetAll;

/// <summary>
/// Summary data transfer object representing a league with its identifier and display name.
/// </summary>
public record LeagueSummaryDto(Guid Id, string LeagueIdentifier, string LeagueName);
