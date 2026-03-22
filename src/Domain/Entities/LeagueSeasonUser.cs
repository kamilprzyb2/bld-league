using BldLeague.Domain.Interfaces;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents a user assigned to a given <see cref="LeagueSeason"/>.
/// </summary>
public class LeagueSeasonUser : IIdentifiable
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
    public User User { get; init; } = null!;
    
    /// <summary>
    /// Associated user ID.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Factory method for league season user.
    /// </summary>
    /// <param name="leagueSeasonId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static LeagueSeasonUser Create(Guid leagueSeasonId, Guid userId)
    {
        return new LeagueSeasonUser
        {
            Id = Guid.CreateVersion7(),
            LeagueSeasonId = leagueSeasonId,
            UserId = userId,
        };
    }
}