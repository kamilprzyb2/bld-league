using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Statistics.GetSeasonRecords;

/// <summary>
/// Per-season best single and best Ao5 records.
/// </summary>
public record SeasonRecordDto(
    int SeasonNumber,
    string SeasonName,
    SolveResult? BestSingle,
    Guid? BestSingleUserId,
    string? BestSingleUserFullName,
    SolveResult? BestAverage,
    Guid? BestAverageUserId,
    string? BestAverageUserFullName,
    int ParticipantCount,
    int ValidSolves,
    int Attempts);
