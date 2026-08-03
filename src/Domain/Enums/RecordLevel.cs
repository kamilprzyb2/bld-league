namespace BldLeague.Domain.Enums;

/// <summary>
/// Hierarchical record level of a result at the time it was achieved.
/// A league (site-wide) record always implies a personal record.
/// </summary>
public enum RecordLevel
{
    /// <summary>
    /// Not a record.
    /// </summary>
    None = 0,

    /// <summary>
    /// Personal record — best (or tied-best) result of the user's own history up to and including the round.
    /// </summary>
    Personal = 1,

    /// <summary>
    /// League record — best (or tied-best) result site-wide up to and including the round.
    /// </summary>
    League = 2,
}
