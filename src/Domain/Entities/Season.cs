using BldLeague.Domain.Interfaces;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents a single league season.
/// </summary>
public class Season : IIdentifiable
{
    /// <inheritdoc />
    public Guid Id { get; init; }
    
    /// <summary>
    /// Numerical season number.
    /// </summary>
    public int SeasonNumber { get; set; }
    
    /// <summary>
    /// Season name (e.g. "Sezon 1").
    /// </summary>
    public string SeasonName
    {
        get
        {
            field ??= $"Sezon {SeasonNumber}";
            return  field;
        }
    }
    
    /// <summary>
    /// League seasons associated with this season.
    /// </summary>
    public ICollection<LeagueSeason> LeagueSeasons { get; set; } = new List<LeagueSeason>();
    
    /// <summary>
    /// Rounds contained in the season.
    /// </summary>
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
    
    /// <summary>
    /// Factory method for the season.
    /// </summary>
    /// <param name="seasonNumber">Numerical season number.</param>
    /// <returns>New season.</returns>
    public static Season Create(int seasonNumber)
        => new Season
        {
            Id = Guid.CreateVersion7(),
            SeasonNumber = seasonNumber,
        };
}