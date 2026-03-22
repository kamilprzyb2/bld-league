using BldLeague.Application.Queries.Rounds.GetScrambles;

namespace BldLeague.Application.Queries.Rounds.GetDetail;

/// <summary>
/// Detailed data transfer object for a round, containing scrambles and the ordered list of player standings.
/// </summary>
public class RoundDetailDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ScrambleDto> Scrambles { get; set; } = new List<ScrambleDto>();
    public required List<RoundStandingDto> Standings { get; set; }
}
