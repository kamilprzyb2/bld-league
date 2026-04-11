using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.PlayerRankings.GetPlayerRankings;

public record AverageRankingDto(
    int AverageRank,
    string FullName,
    SolveResult BestAverage,
    string RoundLabel,
    SolveResult Solve1,
    SolveResult Solve2,
    SolveResult Solve3,
    SolveResult Solve4,
    SolveResult Solve5
);
