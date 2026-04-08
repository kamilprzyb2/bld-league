namespace BldLeague.Application.Queries.Rounds.GetActiveFormUrl;

/// <summary>
/// Contains the submission form URL and round name for the currently active round.
/// </summary>
public record ActiveRoundFormDto(string Url, string RoundName);
