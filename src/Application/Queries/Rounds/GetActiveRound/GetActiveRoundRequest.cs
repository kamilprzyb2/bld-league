using MediatR;

namespace BldLeague.Application.Queries.Rounds.GetActiveRound;

public record GetActiveRoundRequest : IRequest<ActiveRoundDto?>;
