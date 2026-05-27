using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetActiveRoundLiveDetail;

/// <summary>
/// Request to retrieve the live three-section snapshot (Wyniki / Wgrane, oczekuje na przeciwnika / Brak wyniku)
/// for an active or not-yet-finished round, identified by season ID and round number.
/// </summary>
public record GetActiveRoundLiveDetailRequest(Guid SeasonId, int RoundNumber)
    : IRequest<ActiveRoundLiveDetailDto?>;
