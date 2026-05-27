namespace BldLeague.Application.Queries.Statistics.GetRollingAo12Leaders;

/// <summary>
/// Top rolling-Ao12 leaders — users ordered by their personal-best 12-solve rolling average,
/// computed over every window of 12 consecutive non-DNS solves in league order.
/// </summary>
public record RollingAo12LeadersDto(IReadOnlyList<RollingAo12EntryDto> Entries);
