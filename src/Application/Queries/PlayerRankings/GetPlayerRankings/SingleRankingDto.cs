using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.PlayerRankings.GetPlayerRankings;

public record SingleRankingDto(
    int SingleRank,
    string FullName,
    SolveResult BestSingle,
    string RoundLabel
);
