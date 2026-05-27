namespace BldLeague.Application.Queries.Statistics.GetAccuracyLeaders;

/// <summary>
/// A single accuracy-leader row — owning user, valid solve count, and total attempts
/// (valid + DNF, i.e. excluding DNS). The percentage is computed in the view.
/// </summary>
public record AccuracyEntryDto(
    Guid UserId,
    string FullName,
    int ValidSolves,
    int Attempts);
