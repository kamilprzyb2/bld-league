using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Commands.PlayerRankings.Refresh;

public record BestSinglePerUserDto(Guid UserId, SolveResult Best, Guid RoundId);
