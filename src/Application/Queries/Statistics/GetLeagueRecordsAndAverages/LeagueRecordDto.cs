using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Statistics.GetLeagueRecordsAndAverages;

/// <summary>
/// Per-league average solve time + best single and best Ao5 records across all seasons.
/// </summary>
public record LeagueRecordDto(
    string LeagueIdentifier,
    string LeagueName,
    SolveResult? AverageSolve,
    SolveResult? BestSingle,
    Guid? BestSingleUserId,
    string? BestSingleUserFullName,
    SolveResult? BestAverage,
    Guid? BestAverageUserId,
    string? BestAverageUserFullName);
