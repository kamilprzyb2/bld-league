using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Statistics.GetRollingAo25Leaders;

/// <summary>
/// A single rolling-Ao25 leader row — owning user, best Ao25 they ever achieved,
/// and the 25 solves (in league order) that produced it.
/// </summary>
public record RollingAo25EntryDto(
    Guid UserId,
    string FullName,
    SolveResult BestAo25,
    IReadOnlyList<SolveResult> Window);
