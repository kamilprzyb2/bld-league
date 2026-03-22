using BldLeague.Domain.Interfaces;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents a match in the given round.
/// </summary>
public class Match : IIdentifiable
{
    /// <summary>
    /// Number of solves every user performs in a match.
    /// </summary>
    public const int SOLVES_PER_MATCH = 5;
    
    /// <inheritdoc />
    public Guid Id { get; init; }

    /// <summary>
    /// Associated league season.
    /// </summary>
    public LeagueSeason LeagueSeason { get; set; } = null!;

    /// <summary>
    /// Associated league season ID.
    /// </summary>
    public Guid LeagueSeasonId { get; set; }

    /// <summary>
    /// Associated round.
    /// </summary>
    public Round Round { get; set; } = null!;
    
    /// <summary>
    /// Associated round ID.
    /// </summary>
    public Guid RoundId { get; set; }
    
    /// <summary>
    /// User A participating in the match.
    /// </summary>
    public User UserA { get; set; } = null!;
    
    /// <summary>
    /// ID of <see cref="UserA"/>.
    /// </summary>
    public Guid UserAId { get; set; }
    
    /// <summary>
    /// User B participating in the match.
    /// </summary>
    public User? UserB { get; set; }
    
    /// <summary>
    /// ID of <see cref="UserB"/>.
    /// </summary>
    public Guid? UserBId { get; set; }
    
    /// <summary>
    /// Score of <see cref="UserA"/>.
    /// </summary>
    public int UserAScore { get; set; }
    
    /// <summary>
    /// Score of <see cref="UserB"/>.
    /// </summary>
    public int UserBScore { get; set; }

    /// <summary>
    /// Best solve result of <see cref="UserA"/>.
    /// </summary>
    public SolveResult UserABest { get; set; }
    
    /// <summary>
    /// Best solve result of <see cref="UserB"/>.
    /// </summary>
    public SolveResult UserBBest { get; set; }
    
    /// <summary>
    /// Average of <see cref="UserA"/>.
    /// </summary>
    public SolveResult UserAAverage { get; set; }
    
    /// <summary>
    /// Average of <see cref="UserB"/>.
    /// </summary>
    public SolveResult UserBAverage { get; set; }
    
    /// <summary>
    /// All solves belonging to this match.
    /// </summary>
    public ICollection<Solve> Solves { get; set; } = new List<Solve>();
    
    /// <summary>
    /// Factory method for creating new <see cref="Match"/>.
    /// </summary>
    /// <param name="leagueSeasonId">Associated league season ID.</param>
    /// <param name="roundId">Associated round ID.</param>
    /// <param name="userAId">User A participating in the match.</param>
    /// <param name="userBId">User B participating in the match.</param>
    /// <param name="scoreA">Score of <see cref="UserA"/>.</param>
    /// <param name="scoreB">Score of <see cref="UserB"/>.</param>
    /// <returns></returns>
    public static Match Create(Guid leagueSeasonId, Guid roundId, Guid userAId, Guid? userBId = null,
        int scoreA = 0, int scoreB = 0)
        => new Match
        {
            Id = Guid.CreateVersion7(),
            LeagueSeasonId = leagueSeasonId,
            RoundId = roundId,
            UserAId = userAId,
            UserBId = userBId,
            UserAScore = scoreA,
            UserBScore = scoreB
        };
}