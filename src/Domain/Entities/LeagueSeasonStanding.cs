using BldLeague.Domain.Interfaces;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents user's standing in a given league-season.
/// </summary>
public class LeagueSeasonStanding : IIdentifiable
{
    /// <inheritdoc />
    public Guid Id { get; init; }
    
    /// <summary>
    /// Associated league season.
    /// </summary>
    public LeagueSeason LeagueSeason { get; init; } = null!;
    
    /// <summary>
    /// Associated league season ID.
    /// </summary>
    public Guid LeagueSeasonId { get; init; }
    
    /// <summary>
    /// Associated user.
    /// </summary>
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Associated user ID.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// User's place in the given league-season.
    /// </summary>
    public int Place { get; set; }
    
    /// <summary>
    /// Number of matches played by user in a given league-season.
    /// </summary>
    public int MatchesPlayed { get; set; }
    
    /// <summary>
    /// Number of matches won by user in a given league-season.
    /// </summary>
    public int MatchesWon { get; set; }
    
    /// <summary>
    /// Number of matches tied by user in a given league-season.
    /// </summary>
    public int MatchesTied { get; set; }
    
    /// <summary>
    /// Number of matches lost by user in a given league-season.
    /// </summary>
    public int MatchesLost { get; set; }
    
    /// <summary>
    /// Big points given to the user in the given league-season,
    /// for matches he won or tied.
    /// </summary>
    public int BigPoints { get; set; }
    
    /// <summary>
    /// Bonus points given to the user in the given league-season,
    /// for his best solve in every round.
    /// </summary>
    public int BonusPoints { get; set; }
    
    /// <summary>
    /// Small points given to the user in the given league-season,
    /// for every solve won or best solve of the match.
    /// </summary>
    public int SmallPointsGained { get; set; }
    
    /// <summary>
    /// Small points lost by the user in the given league-season.
    /// </summary>
    public int SmallPointsLost { get; set; }
    
    /// <summary>
    /// Balance of user's small points in the given league-season.
    /// </summary>
    /// <remarks>
    /// <see cref="SmallPointsGained"/> minus <see cref="SmallPointsLost"/>.
    /// </remarks>
    public int SmallPointsBalance { get; set; }
    
    /// <summary>
    /// User's best solve result in the entire league-season.
    /// </summary>
    public SolveResult Best { get; set; }

    /// <summary>
    /// Factory method for creating new <see cref="LeagueSeasonStanding"/>.
    /// </summary>
    /// <param name="leagueSeasonId"></param>
    /// <param name="userId"></param>
    /// <param name="place"></param>
    /// <param name="matchesPlayed"></param>
    /// <param name="matchesWon"></param>
    /// <param name="matchesTied"></param>
    /// <param name="matchesLost"></param>
    /// <param name="bigPoints"></param>
    /// <param name="bonusPoints"></param>
    /// <param name="smallPointsGained"></param>
    /// <param name="smallPointsLost"></param>
    /// <param name="smallPointsBalance"></param>
    /// <param name="best"></param>
    /// <returns></returns>
    public static LeagueSeasonStanding Create(
        Guid leagueSeasonId, Guid userId, int place,
        int matchesPlayed, int matchesWon, int matchesTied, int matchesLost, int bigPoints,
        int bonusPoints, int smallPointsGained, int smallPointsLost, int smallPointsBalance,
        SolveResult best)
    {
        return new LeagueSeasonStanding
        {
            Id = Guid.CreateVersion7(),
            LeagueSeasonId = leagueSeasonId,
            UserId = userId,
            Place = place,
            MatchesPlayed = matchesPlayed,
            MatchesWon = matchesWon,
            MatchesTied = matchesTied,
            MatchesLost = matchesLost,
            BigPoints = bigPoints,
            BonusPoints = bonusPoints,
            SmallPointsGained = smallPointsGained,
            SmallPointsLost = smallPointsLost,
            SmallPointsBalance = smallPointsBalance,
            Best = best
        };
    }
}