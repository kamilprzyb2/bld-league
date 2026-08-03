using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Queries.Users.GetHeadToHeadComparison;

public record CommonRoundDto(
    int SeasonNumber,
    int RoundNumber,
    string LeagueIdentifierA,
    string LeagueIdentifierB,
    SolveResult BestA,
    SolveResult AverageA,
    SolveResult BestB,
    SolveResult AverageB,
    bool FacedEachOther
);
