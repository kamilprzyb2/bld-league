using BldLeague.Domain.Interfaces;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents a scramble for a specific solve position in a round.
/// </summary>
public class Scramble : IIdentifiable
{
    /// <inheritdoc />
    public Guid Id { get; init; }

    /// <summary>
    /// Associated round ID.
    /// </summary>
    public Guid RoundId { get; set; }

    /// <summary>
    /// Associated round.
    /// </summary>
    public Round Round { get; set; } = null!;

    /// <summary>
    /// Position of this scramble within the round (1–<see cref="Match.SOLVES_PER_MATCH"/>).
    /// </summary>
    public int ScrambleNumber { get; set; }

    /// <summary>
    /// The scramble notation (move sequence).
    /// </summary>
    public string Notation { get; set; } = string.Empty;

    /// <summary>
    /// Factory method for <see cref="Scramble"/>.
    /// </summary>
    public static Scramble Create(Guid roundId, int scrambleNumber, string notation)
        => new Scramble
        {
            Id = Guid.CreateVersion7(),
            RoundId = roundId,
            ScrambleNumber = scrambleNumber,
            Notation = notation
        };
}
