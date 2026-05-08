using BldLeague.Application.Queries.Rounds.GetScrambles;

namespace BldLeague.Application.Queries.Matches.GetActiveSubmission;

public class ActiveSubmissionDto
{
    public Guid MatchId { get; init; }
    public Guid RoundId { get; init; }
    public string RoundName { get; init; } = string.Empty;
    public string LeagueIdentifier { get; init; } = string.Empty;
    public string OpponentName { get; init; } = string.Empty;
    public bool HasSubmitted { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public IReadOnlyList<ScrambleDto> Scrambles { get; init; } = [];
    public IReadOnlyList<string> SubmittedSolves { get; init; } = [];
}
