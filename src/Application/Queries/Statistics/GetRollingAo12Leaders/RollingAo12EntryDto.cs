using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Statistics.GetRollingAo12Leaders;

/// <summary>
/// A single rolling-Ao12 leader row — owning user, best Ao12 they ever achieved,
/// and the 12 solves (in league order) that produced it.
/// </summary>
public record RollingAo12EntryDto(
    Guid UserId,
    string FullName,
    SolveResult BestAo12,
    IReadOnlyList<SolveResult> Window);
