using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Leagues.Import;

/// <summary>
/// Request to bulk-import leagues from a list of CSV rows.
/// </summary>
public class ImportLeaguesRequest : IRequest<ImportResult>
{
    public required List<ImportLeagueRow> Rows { get; init; }
}

/// <summary>
/// Represents a single CSV row for a league import, carrying the league identifier.
/// </summary>
public class ImportLeagueRow
{
    public int RowNumber { get; init; }
    public required string LeagueIdentifier { get; init; }
}
