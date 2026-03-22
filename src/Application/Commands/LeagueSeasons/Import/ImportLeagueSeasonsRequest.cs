using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasons.Import;

/// <summary>
/// Request to bulk-import league seasons, optionally with initial user rosters, from a list of CSV rows.
/// </summary>
public class ImportLeagueSeasonsRequest : IRequest<ImportResult>
{
    public required List<ImportLeagueSeasonRow> Rows { get; init; }
}

/// <summary>
/// Represents a single CSV row for a league season import, including season number, league identifier, and optional WCA IDs.
/// </summary>
public class ImportLeagueSeasonRow
{
    public int RowNumber { get; init; }
    public int SeasonNumber { get; init; }
    public required string LeagueIdentifier { get; init; }
    public List<string> WcaIds { get; init; } = [];
}
