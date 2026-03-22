using BldLeague.Application.Queries.Matches.GetMatchExport;
using MediatR;

namespace BldLeague.Application.Queries.Matches.GetMatchExport;

/// <summary>
/// Request to retrieve match data for CSV export, optionally filtered by season number, league identifier, and round number.
/// </summary>
public class GetMatchExportRequest : IRequest<IReadOnlyCollection<MatchExportRowDto>>
{
    public int? SeasonNumber { get; init; }
    public string? LeagueIdentifier { get; init; }
    public int? RoundNumber { get; init; }
}
