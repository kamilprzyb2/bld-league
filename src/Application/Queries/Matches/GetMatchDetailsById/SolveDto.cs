using BldLeague.Application.Validation;

namespace BldLeague.Application.Queries.Matches.GetMatchDetailsById;

/// <summary>
/// Data transfer object for a single solve result, used as input when creating or editing a match.
/// </summary>
public class SolveDto
{
    [SolveResult]
    public string? Result { get; init; } = string.Empty;
}
