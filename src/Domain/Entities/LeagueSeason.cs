using BldLeague.Domain.Interfaces;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents properties of a single season in a single league.
/// </summary>
public class LeagueSeason : IIdentifiable
{
    /// <inheritdoc />
    public Guid Id { get; init; }
    
    /// <summary>
    /// Associated league ID.
    /// </summary>
    public Guid LeagueId { get; set; }

    /// <summary>
    /// Associated league.
    /// </summary>
    public League League { get; set; } = null!;

    /// <summary>
    /// Associated Season ID.
    /// </summary>
    public Guid SeasonId { get; set; }

    /// <summary>
    /// Associated Season.
    /// </summary>
    public Season Season { get; set; } = null!;

    /// <summary>
    /// League season users associated with this league season.
    /// </summary>
    public ICollection<LeagueSeasonUser> LeagueSeasonUsers { get; set; } = new List<LeagueSeasonUser>();
    
    /// <summary>
    /// League season standings associated with this league season.
    /// </summary>
    public ICollection<LeagueSeasonStanding> LeagueSeasonStandings { get; set; } = new List<LeagueSeasonStanding>();

    /// <summary>
    /// Matches played in this league season.
    /// </summary>
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    
    /// <summary>
    /// Number of top-placed players to display as promoted. Zero means no promotion icons are shown.
    /// </summary>
    public int PromotionCount { get; set; }

    /// <summary>
    /// Number of bottom-placed players to display as relegated. Zero means no relegation icons are shown.
    /// </summary>
    public int RelegationCount { get; set; }

    /// <summary>
    /// Factory method for LeagueSeason.
    /// </summary>
    /// <param name="league">Associated league.</param>
    /// <param name="season">Associated Season.</param>
    /// <param name="promotionCount">Number of promoted places (default 0).</param>
    /// <param name="relegationCount">Number of relegated places (default 0).</param>
    /// <returns></returns>
    public static LeagueSeason Create(League league, Season season, int promotionCount = 0, int relegationCount = 0)
        => new LeagueSeason
        {
            Id = Guid.CreateVersion7(),
            League = league,
            LeagueId = league.Id,
            Season = season,
            SeasonId = season.Id,
            PromotionCount = promotionCount,
            RelegationCount = relegationCount
        };
}
