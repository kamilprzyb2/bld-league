namespace BldLeague.Application.Queries.Statistics.GetStreakLeaders;

/// <summary>
/// Top streak leader pair: longest solve streak and longest win streak.
/// </summary>
public record StreakLeadersDto(StreakDto? Solve, StreakDto? Win);

/// <summary>
/// A single streak entry — owning user and streak length.
/// </summary>
public record StreakDto(Guid UserId, string FullName, int Length);
