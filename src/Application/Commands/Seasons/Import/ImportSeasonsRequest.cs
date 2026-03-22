using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Seasons.Import;

/// <summary>
/// Request to bulk-import seasons from a list of CSV rows.
/// </summary>
public class ImportSeasonsRequest : IRequest<ImportResult>
{
    public required List<ImportSeasonRow> Rows { get; init; }
}

/// <summary>
/// Represents a single CSV row for a season import, carrying the season number.
/// </summary>
public class ImportSeasonRow
{
    public int RowNumber { get; init; }
    public int SeasonNumber { get; init; }
}
