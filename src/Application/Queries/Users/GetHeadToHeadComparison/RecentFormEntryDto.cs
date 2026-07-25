using BldLeague.Application.Common;

namespace BldLeague.Application.Queries.Users.GetHeadToHeadComparison;

public record RecentFormEntryDto(
    MatchOutcome Outcome,
    int SeasonNumber,
    int RoundNumber,
    int SelfScore,
    int OpponentScore,
    string OpponentFullName
);
