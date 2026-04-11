using BldLeague.Domain.Interfaces;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents the all-time ranking for a player, storing their best single and best average.
/// </summary>
public class PlayerRanking : IIdentifiable
{
    /// <inheritdoc />
    public Guid Id { get; init; }

    /// <summary>
    /// Associated user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Associated user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Player's best single result across all rounds. Null if no valid single exists.
    /// </summary>
    public SolveResult? BestSingle { get; set; }

    /// <summary>
    /// Player's rank by best single. Null if excluded from single ranking.
    /// </summary>
    public int? SingleRank { get; set; }

    /// <summary>
    /// Round ID where the best single was achieved. Null if no valid single exists.
    /// </summary>
    public Guid? SingleRoundId { get; set; }

    /// <summary>
    /// Round where the best single was achieved.
    /// </summary>
    public Round? SingleRound { get; set; }

    /// <summary>
    /// Player's best average result across all rounds. Null if no valid average exists.
    /// </summary>
    public SolveResult? BestAverage { get; set; }

    /// <summary>
    /// Player's rank by best average. Null if excluded from average ranking.
    /// </summary>
    public int? AverageRank { get; set; }

    /// <summary>
    /// Round ID where the best average was achieved. Null if no valid average exists.
    /// </summary>
    public Guid? AverageRoundId { get; set; }

    /// <summary>
    /// Round where the best average was achieved.
    /// </summary>
    public Round? AverageRound { get; set; }

    /// <summary>
    /// Solve 1 from the round where the best average was achieved.
    /// </summary>
    public SolveResult? AverageSolve1 { get; set; }

    /// <summary>
    /// Solve 2 from the round where the best average was achieved.
    /// </summary>
    public SolveResult? AverageSolve2 { get; set; }

    /// <summary>
    /// Solve 3 from the round where the best average was achieved.
    /// </summary>
    public SolveResult? AverageSolve3 { get; set; }

    /// <summary>
    /// Solve 4 from the round where the best average was achieved.
    /// </summary>
    public SolveResult? AverageSolve4 { get; set; }

    /// <summary>
    /// Solve 5 from the round where the best average was achieved.
    /// </summary>
    public SolveResult? AverageSolve5 { get; set; }

    /// <summary>
    /// Factory method for creating a new <see cref="PlayerRanking"/> for a given user.
    /// </summary>
    /// <param name="userId">The associated user ID.</param>
    /// <returns>A new <see cref="PlayerRanking"/> with all nullable ranking fields set to null.</returns>
    public static PlayerRanking Create(Guid userId)
        => new PlayerRanking
        {
            Id = Guid.CreateVersion7(),
            UserId = userId
        };
}
