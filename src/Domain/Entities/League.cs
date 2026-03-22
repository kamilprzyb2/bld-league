using BldLeague.Domain.Interfaces;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents a competitive league.
/// </summary>
public class League : IIdentifiable
{
    /// <inheritdoc />
    public Guid Id { get; init; }
    
    /// <summary>
    /// League identifier (e.g. "A", "B").
    /// </summary>
    public string LeagueIdentifier { get; init; } = string.Empty;
    
    /// <summary>
    /// Full league name (e.g. "Liga A").
    /// </summary>
    public string LeagueName 
    {
        get
        {
            field ??= $"Liga {LeagueIdentifier}";
            return  field;
        }
    }

    /// <summary>
    /// League seasons associated with this league.
    /// </summary>
    public ICollection<LeagueSeason> LeagueSeasons { get; set; } = new List<LeagueSeason>();

    /// <summary>
    /// Round standings associated with the league.
    /// </summary>
    public ICollection<RoundStanding> RoundStandings { get; set; } = new List<RoundStanding>();

    /// <summary>
    /// League factory method.
    /// </summary>
    /// <param name="leagueIdentifier">League identifier (e.g. "A", "B").</param>
    /// <returns>New league.</returns>
    public static League Create(string leagueIdentifier)
        => new League
        {
            Id = Guid.CreateVersion7(),
            LeagueIdentifier = leagueIdentifier
        };
}