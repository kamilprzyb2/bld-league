using BldLeague.Domain.Interfaces;

namespace BldLeague.Domain.Entities;

/// <summary>
/// Represents a single round in a season.
/// </summary>
public class Round : IIdentifiable
{
    /// <inheritdoc />
    public Guid Id { get; init; }
    
    /// <summary>
    /// Associated Season ID.
    /// </summary>
    public Guid SeasonId { get; set; }

    /// <summary>
    /// Associated Season.
    /// </summary>
    public Season Season { get; set; } = null!;
    
    /// <summary>
    /// Numerical round number.
    /// </summary>
    public int RoundNumber { get; set; }
    
    /// <summary>
    /// Round name (e.g. "Kolejka 1").
    /// </summary>
    public string RoundName
    {
        get
        {
            field ??= $"Kolejka {RoundNumber}";
            return  field;
        }
    }
    
    /// <summary>
    /// Date and time when the round starts.
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Date and time when the round ends.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Matches contained in the round.
    /// </summary>
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    
    /// <summary>
    /// Optional URL for the external result submission form (e.g. Google Forms).
    /// Temporary — will be removed once in-app submission is implemented.
    /// </summary>
    public string? SubmissionFormUrl { get; set; }

    /// <summary>
    /// Scrambles belonging to the round.
    /// </summary>
    public ICollection<Scramble> Scrambles { get; set; } = new List<Scramble>();

    /// <summary>
    /// Round standings in the round.
    /// </summary>
    public ICollection<RoundStanding> Standings { get; set; } = new List<RoundStanding>();
    
    /// <summary>
    /// Factory method for <see cref="Round"/>.
    /// </summary>
    /// <param name="seasonId"></param>
    /// <param name="roundNumber"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="submissionFormUrl"></param>
    /// <returns></returns>
    public static Round Create(Guid seasonId, int roundNumber, DateTime startDate, DateTime endDate, string? submissionFormUrl = null)
        => new Round
        {
            Id = Guid.CreateVersion7(),
            SeasonId = seasonId,
            RoundNumber = roundNumber,
            StartDate = startDate,
            EndDate = endDate,
            SubmissionFormUrl = submissionFormUrl
        };
}