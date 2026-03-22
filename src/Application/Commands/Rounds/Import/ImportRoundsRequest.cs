using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Rounds.Import;

/// <summary>
/// Request to bulk-import rounds (with optional scrambles) for a given season from a list of CSV rows.
/// </summary>
public class ImportRoundsRequest : IRequest<ImportResult>
{
    public int SeasonNumber { get; init; }
    public required List<ImportRoundRow> Rows { get; init; }
}

/// <summary>
/// Represents a single CSV row for a round import, carrying the round number, date range, and optional scramble notations.
/// </summary>
public class ImportRoundRow
{
    public int RowNumber { get; init; }
    public int RoundNumber { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    /// <summary>
    /// Scramble notations indexed 0 to <see cref="Match.SOLVES_PER_MATCH"/>-1.
    /// Index 0 = scramble number 1. Null or empty entries are skipped.
    /// </summary>
    public string?[] Scrambles { get; init; } = [];
}
