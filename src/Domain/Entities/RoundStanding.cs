using BldLeague.Domain.Interfaces;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents user's standing in a given round.
/// </summary>
public class RoundStanding : IIdentifiable
{
    /// <inheritdoc />
    public Guid Id { get; init; }
    
    /// <summary>
    /// Associated round.
    /// </summary>
    public Round Round { get; set; } = null!;
    
    /// <summary>
    /// Associated round ID.
    /// </summary>
    public Guid RoundId { get; set; }
    
    /// <summary>
    /// Associated user.
    /// </summary>
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Associated user ID.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Cached user's league during the round.
    /// </summary>
    public League League { get; set; } = null!;
    
    /// <summary>
    /// Cached user's league ID during the round.
    /// </summary>
    public Guid LeagueId { get; set; }
    
    /// <summary>
    /// Points assigned to the user in the given round.
    /// </summary>
    public int Points { get; set; }
    
    /// <summary>
    /// User's place in the given round.
    /// </summary>
    public int Place { get; set; }

    /// <summary>
    /// User's solve 1 in the round (cached).
    /// </summary>
    public SolveResult Solve1 { get; set; }
    
    /// <summary>
    /// User's solve 2 in the round (cached).
    /// </summary>
    public SolveResult Solve2 { get; set; }
    
    /// <summary>
    /// User's solve 3 in the round (cached).
    /// </summary>
    public SolveResult Solve3 { get; set; }
    
    /// <summary>
    /// User's solve 4 in the round (cached).
    /// </summary>
    public SolveResult Solve4 { get; set; }
    
    /// <summary>
    /// User's solve 5 in the round (cached).
    /// </summary>
    public SolveResult Solve5 { get; set; }
    
    /// <summary>
    /// User's best solve result in the round.
    /// </summary>
    public SolveResult Best { get; set; }
    
    /// <summary>
    /// User's average in the round.
    /// </summary>
    public SolveResult Average { get; set; }

    /// <summary>
    /// Factory method for creating a new <see cref="RoundStanding"/>.
    /// </summary>
    /// <param name="userId">Associated user ID.</param>
    /// <param name="roundId">Associated round ID.</param>
    /// <param name="leagueId">Cached user's league ID during the round.</param>
    /// <param name="points">Points assigned to the user in the given round.</param>
    /// <param name="place">User's place in the given round.</param>
    /// <param name="solve1">User's solve 1 in the round (cached).</param>
    /// <param name="solve2">User's solve 2 in the round (cached).</param>
    /// <param name="solve3">User's solve 3 in the round (cached).</param>
    /// <param name="solve4">User's solve 4 in the round (cached).</param>
    /// <param name="solve5">User's solve 5 in the round (cached).</param>
    /// <param name="best">User's best solve result in the round.</param>
    /// <param name="average">User's average in the round.</param>
    /// <returns></returns>
    public static RoundStanding Create(
        Guid userId, Guid roundId, Guid leagueId, int points, int place, 
        SolveResult solve1, SolveResult solve2, SolveResult solve3, SolveResult solve4, SolveResult solve5,
        SolveResult best, SolveResult average)
    {
        return new RoundStanding
        {
            Id = Guid.CreateVersion7(),
            RoundId = roundId,
            UserId = userId,
            LeagueId = leagueId,
            Points = points,
            Place = place,
            Solve1 = solve1,
            Solve2 = solve2,
            Solve3 = solve3,
            Solve4 = solve4,
            Solve5 = solve5,
            Best = best,
            Average = average
        };
    }
}