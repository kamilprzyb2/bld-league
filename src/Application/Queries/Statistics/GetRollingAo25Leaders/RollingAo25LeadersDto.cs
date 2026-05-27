namespace BldLeague.Application.Queries.Statistics.GetRollingAo25Leaders;

/// <summary>
/// Top rolling-Ao25 leaders — users ordered by their personal-best 25-solve rolling average,
/// computed over every window of 25 consecutive non-DNS solves in league order.
/// </summary>
public record RollingAo25LeadersDto(IReadOnlyList<RollingAo25EntryDto> Entries);
