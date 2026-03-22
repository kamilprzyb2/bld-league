using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonUsers.Import;

/// <summary>
/// Request to bulk-import league season user assignments from a list of CSV rows.
/// </summary>
public class ImportLeagueSeasonUsersRequest : IRequest<ImportResult>
{
    public required List<ImportLeagueSeasonUserRow> Rows { get; init; }
}

/// <summary>
/// Represents a single CSV row for a league season user import, carrying season, league, and WCA ID data.
/// </summary>
public class ImportLeagueSeasonUserRow
{
    public int RowNumber { get; init; }
    public int SeasonNumber { get; init; }
    public required string LeagueIdentifier { get; init; }
    public required string WcaId { get; init; }
}
