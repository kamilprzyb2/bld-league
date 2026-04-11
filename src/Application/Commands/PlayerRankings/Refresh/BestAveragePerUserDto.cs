using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Commands.PlayerRankings.Refresh;

public record BestAveragePerUserDto(
    Guid UserId,
    SolveResult Average,
    Guid RoundId,
    SolveResult Solve1,
    SolveResult Solve2,
    SolveResult Solve3,
    SolveResult Solve4,
    SolveResult Solve5);
