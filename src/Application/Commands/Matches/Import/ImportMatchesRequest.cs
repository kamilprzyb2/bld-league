using BldLeague.Application.Common;
using BldLeague.Domain.ValueObjects;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Import;

/// <summary>
/// Request to bulk-import matches with solve results from a list of CSV rows.
/// </summary>
public class ImportMatchesRequest : IRequest<ImportResult>
{
    public required List<ImportMatchRow> Rows { get; init; }
}

/// <summary>
/// Represents a single CSV row for a match import, carrying player WCA IDs, solve results, and contextual identifiers.
/// </summary>
public class ImportMatchRow
{
    public int RowNumber { get; init; }
    public int SeasonNumber { get; init; }
    public required string LeagueIdentifier { get; init; }
    public int RoundNumber { get; init; }
    public required string WcaIdA { get; init; }
    public string? WcaIdB { get; init; }
    public required List<SolveResult> SolvesA { get; init; }
    public required List<SolveResult> SolvesB { get; init; }
}
