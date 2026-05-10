namespace BldLeague.Application.Queries.Rounds.GetActiveRound;

public class ActiveRoundSummaryDto
{
    public Guid RoundId { get; init; }
    public int RoundNumber { get; init; }
    public int SeasonNumber { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
