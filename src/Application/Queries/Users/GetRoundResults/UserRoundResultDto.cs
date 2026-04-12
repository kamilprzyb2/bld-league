using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Users.GetRoundResults;

public record UserRoundResultDto(
    int SeasonNumber,
    int RoundNumber,
    string SeasonName,
    string RoundName,
    string LeagueIdentifier,
    Guid SeasonId,
    int? Place,
    SolveResult Best,
    SolveResult Average,
    SolveResult Solve1,
    SolveResult Solve2,
    SolveResult Solve3,
    SolveResult Solve4,
    SolveResult Solve5
);
