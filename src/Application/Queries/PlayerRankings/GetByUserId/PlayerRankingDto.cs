using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.PlayerRankings.GetByUserId;

public record PlayerRankingDto(
    SolveResult? BestSingle,
    int? SingleRank,
    SolveResult? BestAverage,
    int? AverageRank
);
