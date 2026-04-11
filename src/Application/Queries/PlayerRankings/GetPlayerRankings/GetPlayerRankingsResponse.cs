namespace BldLeague.Application.Queries.PlayerRankings.GetPlayerRankings;

public record GetPlayerRankingsResponse(
    IReadOnlyCollection<SingleRankingDto> SingleRankings,
    IReadOnlyCollection<AverageRankingDto> AverageRankings
);
