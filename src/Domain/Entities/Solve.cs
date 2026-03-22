using BldLeague.Domain.Interfaces;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents a single solve in a match.
/// </summary>
public class Solve : IIdentifiable
{
    /// <inheritdoc />
    public Guid Id { get; init; }
    
    /// <summary>
    /// Solve result.
    /// </summary>
    public SolveResult Result { get; set; }
    
    /// <summary>
    /// Index of the solve in the match (e.g. solve 1 in match).
    /// </summary>
    public int Index { get; set; }
    
    /// <summary>
    /// Associated match.
    /// </summary>
    public Match Match { get; set; } = null!;
    
    /// <summary>
    /// Associated match ID.
    /// </summary>
    public Guid MatchId { get; set; }
    
    /// <summary>
    /// Associated user.
    /// </summary>
    public User User { get; set; } = null!;
        
    /// <summary>
    /// Associated user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Factory method for creating a new <see cref="Solve"/>.
    /// </summary>
    /// <param name="result">Solve result.</param>
    /// <param name="index">Index of the solve in the match (e.g. solve 1 in match).</param>
    /// <param name="userId">Associated user ID.</param>
    /// <param name="matchId">Associated match ID.</param>
    /// <returns></returns>
    public static Solve Create(SolveResult result, int index, Guid userId, Guid matchId)
        => new Solve
        {
            Id = Guid.CreateVersion7(),
            Index = index,
            Result = result,
            MatchId = matchId,
            UserId = userId,
        };
}