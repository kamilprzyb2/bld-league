namespace BldLeague.Application.Queries.Statistics.GetAccuracyLeaders;

/// <summary>
/// Top accuracy leaders — users ordered by ValidSolves / Attempts ratio descending.
/// Limited to users with at least the configured minimum number of attempts.
/// </summary>
public record AccuracyLeadersDto(IReadOnlyList<AccuracyEntryDto> Entries);
