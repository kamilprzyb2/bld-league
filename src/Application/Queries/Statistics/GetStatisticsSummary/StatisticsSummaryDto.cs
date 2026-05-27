namespace BldLeague.Application.Queries.Statistics.GetStatisticsSummary;

/// <summary>
/// Global counters for the home page and statistics page header tiles. Mixed semantics:
/// solve and match counters update live (reflecting submissions in the active round),
/// while the participants counter stays scoped to finished matches.
/// </summary>
/// <param name="ValidSolves">Live count of valid (non-DNF, non-DNS) solves across all matches — submitted solves on active rounds are included immediately.</param>
/// <param name="Attempts">Live count of valid + DNF solves across all matches (DNS excluded) — submitted solves on active rounds are included immediately.</param>
/// <param name="Matches">Live count of matches that have ended (round end date passed) or had both sides submit, including BYE matches.</param>
/// <param name="Participants">Count of distinct users that have at least one finished-match appearance. Finished-only — does not change until a round flips to finished.</param>
public record StatisticsSummaryDto(int ValidSolves, int Attempts, int Matches, int Participants);
