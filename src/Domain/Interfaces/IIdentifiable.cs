namespace BldLeague.Domain.Interfaces;

/// <summary>
/// Interface for entities that have a unique identifier.
/// </summary>
public interface IIdentifiable
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public Guid Id { get; init; }
}
