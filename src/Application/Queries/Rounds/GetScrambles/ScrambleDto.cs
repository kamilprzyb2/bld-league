namespace BldLeague.Application.Queries.Rounds.GetScrambles;

/// <summary>
/// Data transfer object representing a single scramble, identified by its position number and move-sequence notation.
/// </summary>
public class ScrambleDto
{
    public int ScrambleNumber { get; set; }
    public string Notation { get; set; } = string.Empty;
}
