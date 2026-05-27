using BldLeague.Application.Queries.Statistics.GetAccuracyLeaders;
using BldLeague.Application.Queries.Statistics.GetLeagueRecordsAndAverages;
using BldLeague.Application.Queries.Statistics.GetSeasonRecords;
using BldLeague.Application.Queries.Statistics.GetStatisticsSummary;
using BldLeague.Domain.ValueObjects;

namespace BldLeague.Application.Abstractions.Repositories;

/// <summary>
/// Raw projections for the global statistics surface. Most queries are scoped to
/// finished rounds only — see per-method docs. Callers pass <c>localToday</c> from
/// <c>RoundClock.LocalToday()</c> and the repository applies <c>round.EndDate &lt; localToday</c>
/// (or a per-match equivalent) where appropriate.
/// </summary>
public interface IStatisticsRepository
{
    /// <summary>
    /// Returns aggregate counters with mixed semantics: <c>ValidSolves</c> and <c>Attempts</c>
    /// are live (every submitted solve counts, regardless of round status); <c>Matches</c> is
    /// live too — counted when the round has ended or both sides have submitted; <c>Participants</c>
    /// stays finished-only (distinct users with at least one finished-match appearance).
    /// </summary>
    Task<StatisticsSummaryDto> GetSummaryAsync(DateTime localToday);

    /// <summary>
    /// Returns UTC submission timestamps from finished matches plus inclusion ratios for the footnote.
    /// </summary>
    Task<SubmissionTimestampsProjection> GetSubmissionTimestampsAsync(DateTime localToday);

    /// <summary>
    /// Returns the raw centisecond values of every valid solve (not DNF, not DNS) in finished matches.
    /// </summary>
    Task<IReadOnlyList<int>> GetValidSolveCentisecondsAsync(DateTime localToday);

    /// <summary>
    /// Returns (UserAScore, UserBScore) for every finished match vs a real opponent.
    /// BYE matches (UserBId == null) are excluded.
    /// </summary>
    Task<IReadOnlyList<(int UserAScore, int UserBScore)>> GetFinishedMatchScoresAsync(DateTime localToday);

    /// <summary>
    /// Returns per-season record rows (best single + best Ao5 with owning user) for every season with at least one finished round.
    /// </summary>
    Task<IReadOnlyList<SeasonRecordDto>> GetSeasonRecordsAsync(DateTime localToday);

    /// <summary>
    /// Returns per-league record + average rows for every league with at least one finished match.
    /// </summary>
    Task<IReadOnlyList<LeagueRecordDto>> GetLeagueRecordsAndAveragesAsync(DateTime localToday);

    /// <summary>
    /// Returns solves grouped by user, ordered chronologically within each user, across all finished matches.
    /// </summary>
    Task<IReadOnlyList<UserSolveStreakGroup>> GetSolvesGroupedByUserAsync(DateTime localToday);

    /// <summary>
    /// Returns matches grouped by user with self/opponent scores, ordered chronologically within each user.
    /// BYE matches are excluded (skipped in win-streak logic per spec).
    /// </summary>
    Task<IReadOnlyList<UserMatchStreakGroup>> GetMatchesGroupedByUserAsync(DateTime localToday);

    /// <summary>
    /// Returns the accuracy leaders — users sorted by ValidSolves / Attempts ratio descending,
    /// then by Attempts descending, then by UserId ascending. Only users with at least
    /// <paramref name="minAttempts"/> attempts (non-DNS solves) are included. Returns every qualifier;
    /// the view decides how many to surface.
    /// </summary>
    Task<IReadOnlyList<AccuracyEntryDto>> GetAccuracyLeadersAsync(DateTime localToday, int minAttempts);

    /// <summary>
    /// Returns non-DNS solves grouped by user, ordered chronologically (season → round → solve index) within each user,
    /// across all finished matches. Used to compute rolling Ao12 windows.
    /// </summary>
    Task<IReadOnlyList<UserSolveLeagueGroup>> GetNonDnsSolvesGroupedByUserAsync(DateTime localToday);
}

/// <summary>
/// Submission-timestamp projection — raw UTC timestamps plus inclusion ratios for the footnote.
/// </summary>
public record SubmissionTimestampsProjection(
    IReadOnlyList<DateTime> Timestamps,
    int IncludedMatches,
    int TotalMatches);

/// <summary>
/// All chronologically-ordered solves performed by a single user across finished matches.
/// </summary>
public record UserSolveStreakGroup(Guid UserId, string FullName, IReadOnlyList<SolveResult> Solves);

/// <summary>
/// All chronologically-ordered matches vs a real opponent for a single user across finished matches.
/// </summary>
public record UserMatchStreakGroup(Guid UserId, string FullName, IReadOnlyList<(int Self, int Opponent)> Matches);

/// <summary>
/// All non-DNS solves performed by a single user across finished matches, ordered by league sequence
/// (season number → round number → solve index).
/// </summary>
public record UserSolveLeagueGroup(Guid UserId, string FullName, IReadOnlyList<SolveResult> Solves);
