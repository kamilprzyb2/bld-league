namespace BldLeague.Application.Queries.Rounds.GetActiveRoundLiveDetail;

/// <summary>
/// Snapshot of a round that is currently active (or not yet started): users are bucketed into
/// matches whose both sides have submitted (full reveal), users who submitted alone (no reveal),
/// and users with an active match who haven't submitted yet (no reveal).
/// </summary>
public class ActiveRoundLiveDetailDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required List<LiveRoundRowDto> FinishedRows { get; set; }
    public required List<LiveRoundRowDto> SubmittedAloneRows { get; set; }
    public required List<LiveRoundRowDto> PendingRows { get; set; }
}
