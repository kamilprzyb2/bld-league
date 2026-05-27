using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Statistics.GetAccuracyLeaders;
using BldLeague.Application.Queries.Statistics.GetLeagueRecordsAndAverages;
using BldLeague.Application.Queries.Statistics.GetSeasonRecords;
using BldLeague.Application.Queries.Statistics.GetStatisticsSummary;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class StatisticsRepository(AppDbContext context) : IStatisticsRepository
{
    private IQueryable<Match> FinishedMatches(DateTime localToday)
        => context.Set<Match>().Where(m => m.Round.EndDate < localToday);

    private IQueryable<Solve> FinishedSolves(DateTime localToday)
        => context.Set<Solve>().Where(s => s.Match.Round.EndDate < localToday);

    public async Task<StatisticsSummaryDto> GetSummaryAsync(DateTime localToday)
    {
        // Live: any submitted solve, regardless of round status. SolveResult stores -1 for DNF and -2 for DNS;
        // valid solves have non-negative centiseconds.
        var validCount = await context.Set<Solve>().CountAsync(s => s.Result >= 0);
        var attemptsCount = await context.Set<Solve>().CountAsync(s => s.Result != -2);

        // Live: a match is "played" when its round has ended OR both sides have submitted.
        // The OR-clause mirrors Match.BothSidesSubmitted; inlined here so EF translates it to SQL.
        var matchCount = await context.Set<Match>()
            .CountAsync(m =>
                m.Round.EndDate < localToday
                || (m.UserASubmittedAt != null && (m.UserBId == null || m.UserBSubmittedAt != null)));

        // Unchanged: finished-only distinct participants.
        var participantsA = FinishedMatches(localToday).Select(m => m.UserAId);
        var participantsB = FinishedMatches(localToday)
            .Where(m => m.UserBId != null)
            .Select(m => m.UserBId!.Value);
        var participantCount = await participantsA.Union(participantsB).CountAsync();

        return new StatisticsSummaryDto(
            ValidSolves: validCount,
            Attempts: attemptsCount,
            Matches: matchCount,
            Participants: participantCount);
    }

    public async Task<SubmissionTimestampsProjection> GetSubmissionTimestampsAsync(DateTime localToday)
    {
        var totalMatches = await FinishedMatches(localToday).CountAsync();

        var matchTimestamps = await FinishedMatches(localToday)
            .Select(m => new { m.UserASubmittedAt, m.UserBSubmittedAt })
            .ToListAsync();

        var timestamps = new List<DateTime>(matchTimestamps.Count * 2);
        int included = 0;
        foreach (var row in matchTimestamps)
        {
            bool any = false;
            if (row.UserASubmittedAt.HasValue)
            {
                timestamps.Add(DateTime.SpecifyKind(row.UserASubmittedAt.Value, DateTimeKind.Utc));
                any = true;
            }
            if (row.UserBSubmittedAt.HasValue)
            {
                timestamps.Add(DateTime.SpecifyKind(row.UserBSubmittedAt.Value, DateTimeKind.Utc));
                any = true;
            }
            if (any) included++;
        }

        return new SubmissionTimestampsProjection(timestamps, included, totalMatches);
    }

    public async Task<IReadOnlyList<int>> GetValidSolveCentisecondsAsync(DateTime localToday)
        => await FinishedSolves(localToday)
            .Where(s => s.Result >= 0)
            .Select(s => (int)s.Result)
            .ToListAsync();

    public async Task<IReadOnlyList<(int UserAScore, int UserBScore)>> GetFinishedMatchScoresAsync(DateTime localToday)
    {
        var rows = await FinishedMatches(localToday)
            .Where(m => m.UserBId != null)
            .Select(m => new { m.UserAScore, m.UserBScore })
            .ToListAsync();
        return rows.Select(r => (r.UserAScore, r.UserBScore)).ToList();
    }

    public async Task<IReadOnlyList<SeasonRecordDto>> GetSeasonRecordsAsync(DateTime localToday)
    {
        var seasons = await context.Set<Season>()
            .Where(s => s.Rounds.Any(r => r.EndDate < localToday))
            .Select(s => new { s.Id, s.SeasonNumber })
            .OrderBy(s => s.SeasonNumber)
            .ToListAsync();

        var results = new List<SeasonRecordDto>(seasons.Count);

        foreach (var season in seasons)
        {
            // Best single — minimum valid solve in finished matches of this season;
            // tie-break by user Guid (v7 is creation-time-ordered, so the earliest user wins).
            var bestSingle = await FinishedSolves(localToday)
                .Where(s => s.Match.Round.SeasonId == season.Id && s.Result >= 0)
                .OrderBy(s => (int)s.Result)
                .ThenBy(s => s.UserId)
                .Select(s => new { Cs = (int)s.Result, s.UserId, FullName = s.User.FullName })
                .FirstOrDefaultAsync();

            // Best Ao5 — minimum valid match average for either player in finished matches of this season.
            var aoCandidates = await FinishedMatches(localToday)
                .Where(m => m.Round.SeasonId == season.Id)
                .Select(m => new
                {
                    m.UserAId,
                    m.UserBId,
                    UserAFullName = m.UserA.FullName,
                    UserBFullName = m.UserB != null ? m.UserB.FullName : null,
                    AverageACs = (int)m.UserAAverage,
                    AverageBCs = (int)m.UserBAverage,
                })
                .ToListAsync();

            var bestAo5 = PickBestAverage(aoCandidates.Select(c => (c.UserAId, c.UserBId, c.UserAFullName, c.UserBFullName, c.AverageACs, c.AverageBCs)));

            // Distinct participants in this season's finished matches — UserAId always present,
            // UserBId only counted when non-null (BYEs don't add a phantom participant).
            var participantsA = FinishedMatches(localToday)
                .Where(m => m.Round.SeasonId == season.Id)
                .Select(m => m.UserAId);
            var participantsB = FinishedMatches(localToday)
                .Where(m => m.Round.SeasonId == season.Id && m.UserBId != null)
                .Select(m => m.UserBId!.Value);
            var participantCount = await participantsA.Union(participantsB).CountAsync();

            var validSolves = await FinishedSolves(localToday)
                .CountAsync(s => s.Match.Round.SeasonId == season.Id && s.Result >= 0);
            var attempts = await FinishedSolves(localToday)
                .CountAsync(s => s.Match.Round.SeasonId == season.Id && s.Result != -2);

            results.Add(new SeasonRecordDto(
                SeasonNumber: season.SeasonNumber,
                SeasonName: $"Sezon {season.SeasonNumber}",
                BestSingle: bestSingle != null ? SolveResult.FromCentiseconds(bestSingle.Cs) : null,
                BestSingleUserId: bestSingle?.UserId,
                BestSingleUserFullName: bestSingle?.FullName,
                BestAverage: bestAo5 != null ? SolveResult.FromCentiseconds(bestAo5.Value.cs) : null,
                BestAverageUserId: bestAo5?.userId,
                BestAverageUserFullName: bestAo5?.fullName,
                ParticipantCount: participantCount,
                ValidSolves: validSolves,
                Attempts: attempts));
        }

        return results;
    }

    public async Task<IReadOnlyList<LeagueRecordDto>> GetLeagueRecordsAndAveragesAsync(DateTime localToday)
    {
        var leagues = await context.Set<League>()
            .Where(l => l.LeagueSeasons.Any(ls => ls.Matches.Any(m => m.Round.EndDate < localToday)))
            .Select(l => new { l.Id, l.LeagueIdentifier })
            .OrderBy(l => l.LeagueIdentifier)
            .ToListAsync();

        var results = new List<LeagueRecordDto>(leagues.Count);

        foreach (var league in leagues)
        {
            // Arithmetic mean of valid solve centiseconds across all finished matches in this league.
            var avgRow = await FinishedSolves(localToday)
                .Where(s => s.Match.LeagueSeason.LeagueId == league.Id && s.Result >= 0)
                .GroupBy(_ => 1)
                .Select(g => new { Avg = g.Average(s => (double)(int)s.Result), Count = g.Count() })
                .FirstOrDefaultAsync();

            var bestSingle = await FinishedSolves(localToday)
                .Where(s => s.Match.LeagueSeason.LeagueId == league.Id && s.Result >= 0)
                .OrderBy(s => (int)s.Result)
                .ThenBy(s => s.UserId)
                .Select(s => new { Cs = (int)s.Result, s.UserId, FullName = s.User.FullName })
                .FirstOrDefaultAsync();

            var aoCandidates = await FinishedMatches(localToday)
                .Where(m => m.LeagueSeason.LeagueId == league.Id)
                .Select(m => new
                {
                    m.UserAId,
                    m.UserBId,
                    UserAFullName = m.UserA.FullName,
                    UserBFullName = m.UserB != null ? m.UserB.FullName : null,
                    AverageACs = (int)m.UserAAverage,
                    AverageBCs = (int)m.UserBAverage,
                })
                .ToListAsync();

            var bestAo5 = PickBestAverage(aoCandidates.Select(c => (c.UserAId, c.UserBId, c.UserAFullName, c.UserBFullName, c.AverageACs, c.AverageBCs)));

            SolveResult? averageSolve = avgRow != null && avgRow.Count > 0
                ? SolveResult.FromCentiseconds((int)Math.Round(avgRow.Avg))
                : null;

            results.Add(new LeagueRecordDto(
                LeagueIdentifier: league.LeagueIdentifier,
                LeagueName: $"Liga {league.LeagueIdentifier}",
                AverageSolve: averageSolve,
                BestSingle: bestSingle != null ? SolveResult.FromCentiseconds(bestSingle.Cs) : null,
                BestSingleUserId: bestSingle?.UserId,
                BestSingleUserFullName: bestSingle?.FullName,
                BestAverage: bestAo5 != null ? SolveResult.FromCentiseconds(bestAo5.Value.cs) : null,
                BestAverageUserId: bestAo5?.userId,
                BestAverageUserFullName: bestAo5?.fullName));
        }

        return results;
    }

    public async Task<IReadOnlyList<UserSolveStreakGroup>> GetSolvesGroupedByUserAsync(DateTime localToday)
    {
        var rows = await FinishedSolves(localToday)
            .Select(s => new
            {
                s.UserId,
                FullName = s.User.FullName,
                SeasonNumber = s.Match.Round.Season.SeasonNumber,
                RoundNumber = s.Match.Round.RoundNumber,
                s.Index,
                ResultCs = (int)s.Result,
            })
            .ToListAsync();

        return rows
            .GroupBy(r => new { r.UserId, r.FullName })
            .Select(g => new UserSolveStreakGroup(
                g.Key.UserId,
                g.Key.FullName,
                g.OrderBy(r => r.SeasonNumber)
                 .ThenBy(r => r.RoundNumber)
                 .ThenBy(r => r.Index)
                 .Select(r => SolveResult.FromCentiseconds(r.ResultCs))
                 .ToList()))
            .ToList();
    }

    public async Task<IReadOnlyList<UserMatchStreakGroup>> GetMatchesGroupedByUserAsync(DateTime localToday)
    {
        // Only matches vs a real opponent — BYE matches are skipped (do not break the streak).
        var rows = await FinishedMatches(localToday)
            .Where(m => m.UserBId != null)
            .Select(m => new
            {
                m.UserAId,
                UserAFullName = m.UserA.FullName,
                UserBId = m.UserBId!.Value,
                UserBFullName = m.UserB!.FullName,
                m.UserAScore,
                m.UserBScore,
                SeasonNumber = m.Round.Season.SeasonNumber,
                RoundNumber = m.Round.RoundNumber,
            })
            .ToListAsync();

        var perUser = new Dictionary<Guid, (string FullName, List<(int Season, int Round, int Self, int Opponent)> Entries)>();
        foreach (var r in rows)
        {
            if (!perUser.TryGetValue(r.UserAId, out var aBucket))
            {
                aBucket = (r.UserAFullName, new List<(int, int, int, int)>());
                perUser[r.UserAId] = aBucket;
            }
            aBucket.Entries.Add((r.SeasonNumber, r.RoundNumber, r.UserAScore, r.UserBScore));

            if (!perUser.TryGetValue(r.UserBId, out var bBucket))
            {
                bBucket = (r.UserBFullName, new List<(int, int, int, int)>());
                perUser[r.UserBId] = bBucket;
            }
            bBucket.Entries.Add((r.SeasonNumber, r.RoundNumber, r.UserBScore, r.UserAScore));
        }

        return perUser
            .Select(kvp => new UserMatchStreakGroup(
                kvp.Key,
                kvp.Value.FullName,
                kvp.Value.Entries
                    .OrderBy(e => e.Season)
                    .ThenBy(e => e.Round)
                    .Select(e => (e.Self, e.Opponent))
                    .ToList()))
            .ToList();
    }

    public async Task<IReadOnlyList<AccuracyEntryDto>> GetAccuracyLeadersAsync(DateTime localToday, int minAttempts)
    {
        // Group all attempts (non-DNS solves) per user and count valid (non-negative) entries.
        // Server-side projection — ratio comparison and tie-break by Guid all expressible in EF Core.
        var rows = await FinishedSolves(localToday)
            .Where(s => s.Result != -2)
            .GroupBy(s => new { s.UserId, FullName = s.User.FullName })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.FullName,
                Attempts = g.Count(),
                ValidSolves = g.Count(s => s.Result >= 0),
            })
            .Where(r => r.Attempts >= minAttempts)
            .ToListAsync();

        return rows
            .OrderByDescending(r => (double)r.ValidSolves / r.Attempts)
            .ThenByDescending(r => r.Attempts)
            .ThenBy(r => r.UserId)
            .Select(r => new AccuracyEntryDto(r.UserId, r.FullName, r.ValidSolves, r.Attempts))
            .ToList();
    }

    public async Task<IReadOnlyList<UserSolveLeagueGroup>> GetNonDnsSolvesGroupedByUserAsync(DateTime localToday)
    {
        var rows = await FinishedSolves(localToday)
            .Where(s => s.Result != -2)
            .Select(s => new
            {
                s.UserId,
                FullName = s.User.FullName,
                SeasonNumber = s.Match.Round.Season.SeasonNumber,
                RoundNumber = s.Match.Round.RoundNumber,
                s.Index,
                ResultCs = (int)s.Result,
            })
            .ToListAsync();

        return rows
            .GroupBy(r => new { r.UserId, r.FullName })
            .Select(g => new UserSolveLeagueGroup(
                g.Key.UserId,
                g.Key.FullName,
                g.OrderBy(r => r.SeasonNumber)
                 .ThenBy(r => r.RoundNumber)
                 .ThenBy(r => r.Index)
                 .Select(r => SolveResult.FromCentiseconds(r.ResultCs))
                 .ToList()))
            .ToList();
    }

    /// <summary>
    /// Picks the best (minimum) valid match average across rows, with tie-break by user Guid
    /// (v7 is creation-time-ordered, so the earliest user wins on a tie).
    /// </summary>
    private static (int cs, Guid userId, string fullName)? PickBestAverage(
        IEnumerable<(Guid UserAId, Guid? UserBId, string UserAFullName, string? UserBFullName, int AverageACs, int AverageBCs)> rows)
    {
        (int cs, Guid userId, string fullName)? best = null;
        foreach (var row in rows)
        {
            if (row.AverageACs >= 0)
            {
                if (best == null
                    || row.AverageACs < best.Value.cs
                    || (row.AverageACs == best.Value.cs && row.UserAId.CompareTo(best.Value.userId) < 0))
                {
                    best = (row.AverageACs, row.UserAId, row.UserAFullName);
                }
            }
            if (row.AverageBCs >= 0 && row.UserBId.HasValue && row.UserBFullName != null)
            {
                if (best == null
                    || row.AverageBCs < best.Value.cs
                    || (row.AverageBCs == best.Value.cs && row.UserBId.Value.CompareTo(best.Value.userId) < 0))
                {
                    best = (row.AverageBCs, row.UserBId.Value, row.UserBFullName);
                }
            }
        }
        return best;
    }
}
