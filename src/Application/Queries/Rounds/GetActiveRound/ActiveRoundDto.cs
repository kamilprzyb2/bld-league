namespace BldLeague.Application.Queries.Rounds.GetActiveRound;

public class ActiveRoundDto
{
    public int RoundNumber { get; init; }
    public DateTime EndsAtUtc { get; init; }
}
