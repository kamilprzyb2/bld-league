using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetActiveFormUrl;

/// <summary>
/// Request to get the submission form URL for the currently active round (StartDate &lt;= UtcNow &lt;= EndDate).
/// Returns null when no round is active or the active round has no form URL set.
/// </summary>
public class GetActiveRoundFormUrlRequest : IRequest<ActiveRoundFormDto?>;
