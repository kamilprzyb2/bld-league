using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Rounds.GetActiveRound;
using BldLeague.Application.Queries.Rounds.GetActiveRoundLiveDetail;
using BldLeague.Application.Queries.Rounds.GetAll;
using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using BldLeague.Application.Queries.Rounds.GetDetail;
using BldLeague.Application.Queries.Rounds.GetScrambles;
using BldLeague.Domain.Entities;
using BldLeague.Domain.ValueObjects;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class RoundRepository(AppDbContext context) :
    ReadWriteRepositoryBase<Round>(context), IRoundRepository
{
    public async Task<IReadOnlyCollection<RoundSummaryDto>> GetRoundSummariesBySeasonIdAsync(Guid seasonId)
        => await DbSet
            .Where(r => r.SeasonId == seasonId)
            .OrderBy(r => r.RoundNumber)
            .Select(r => new RoundSummaryDto(r.Id, r.SeasonId, r.RoundNumber, r.StartDate, r.EndDate))
            .ToListAsync();

    public async Task<IReadOnlyCollection<RoundAdminSummaryDto>> GetAllRoundSummariesAsync()
        => await DbSet
            .OrderByDescending(r => r.Season.SeasonNumber)
            .ThenBy(r => r.RoundNumber)
            .Select(r => new RoundAdminSummaryDto(r.Id, r.SeasonId, r.Season.SeasonNumber, r.RoundNumber, r.StartDate, r.EndDate))
            .ToListAsync();

    public async Task<RoundSummaryDto?> GetSummaryByIdAsync(Guid id)
        => await DbSet
            .Where(r => r.Id == id)
            .Select(r => new RoundSummaryDto(r.Id, r.SeasonId, r.RoundNumber, r.StartDate, r.EndDate))
            .FirstOrDefaultAsync();

    public async Task<int?> GetLatestRoundNumberAsync()
        => await DbSet
            .Select(r => (int?)r.RoundNumber)
            .MaxAsync();

    public async Task<RoundDetailDto?> GetRoundDetailAsync(Guid seasonId, int roundNumber)
        => await DbSet
            .Where(r => r.SeasonId == seasonId && r.RoundNumber == roundNumber)
            .Select(r => new RoundDetailDto
            {
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Scrambles = r.Scrambles
                    .OrderBy(s => s.ScrambleNumber)
                    .Select(s => new ScrambleDto { ScrambleNumber = s.ScrambleNumber, Notation = s.Notation })
                    .ToList(),
                Standings = r.Standings
                    .OrderBy(rs => rs.Place)
                    .ThenBy(rs => rs.User.FullName)
                    .Select(rs => new RoundStandingDto
                    {
                        UserId = rs.UserId,
                        UserFullName = rs.User.FullName,
                        LeagueIdentifier = rs.League.LeagueIdentifier,
                        Solve1 = rs.Solve1,
                        Solve2 = rs.Solve2,
                        Solve3 = rs.Solve3,
                        Solve4 = rs.Solve4,
                        Solve5 = rs.Solve5,
                        Best = rs.Best,
                        Average = rs.Average,
                        Place = rs.Place == int.MaxValue ? null : rs.Place,
                        Points = rs.Points
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

    public async Task<IReadOnlyCollection<ActiveRoundSummaryDto>> GetRoundsActiveOnDateAsync(DateTime localToday)
        => await DbSet
            .Where(r => r.StartDate <= localToday && r.EndDate >= localToday)
            .Select(r => new ActiveRoundSummaryDto
            {
                RoundId = r.Id,
                RoundNumber = r.RoundNumber,
                SeasonNumber = r.Season.SeasonNumber,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
            })
            .ToListAsync();

    public async Task<ActiveRoundLiveDetailDto?> GetActiveRoundLiveDetailAsync(Guid seasonId, int roundNumber)
    {
        var round = await DbSet
            .Where(r => r.SeasonId == seasonId && r.RoundNumber == roundNumber)
            .Select(r => new { r.Id, r.StartDate, r.EndDate })
            .FirstOrDefaultAsync();

        if (round == null)
            return null;

        var matches = await context.Set<Match>()
            .Where(m => m.RoundId == round.Id)
            .Select(m => new LiveRoundMatchProjection
            {
                LeagueIdentifier = m.LeagueSeason.League.LeagueIdentifier,
                UserAId = m.UserAId,
                UserAFullName = m.UserA.FullName,
                UserASubmitted = m.UserASubmittedAt != null,
                UserABest = m.UserABest,
                UserAAverage = m.UserAAverage,
                UserASolves = m.Solves
                    .Where(s => s.UserId == m.UserAId)
                    .OrderBy(s => s.Index)
                    .Select(s => s.Result)
                    .ToList(),
                UserBId = m.UserBId,
                UserBFullName = m.UserB != null ? m.UserB.FullName : null,
                UserBSubmitted = m.UserBSubmittedAt != null,
                UserBBest = m.UserBBest,
                UserBAverage = m.UserBAverage,
                UserBSolves = m.Solves
                    .Where(s => s.UserId == m.UserBId)
                    .OrderBy(s => s.Index)
                    .Select(s => s.Result)
                    .ToList(),
            })
            .ToListAsync();

        var finishedRows = new List<LiveRoundRowDto>();
        var submittedAloneRows = new List<LiveRoundRowDto>();
        var pendingRows = new List<LiveRoundRowDto>();

        foreach (var m in matches)
        {
            var bothSidesSubmitted = m.UserASubmitted && (m.UserBId == null || m.UserBSubmitted);

            // UserA is always present.
            if (bothSidesSubmitted)
            {
                finishedRows.Add(BuildFinishedRow(
                    m.UserAId, m.UserAFullName, m.LeagueIdentifier,
                    m.UserASolves, m.UserABest, m.UserAAverage));
            }
            else if (m.UserASubmitted)
            {
                submittedAloneRows.Add(BuildIdentityRow(
                    m.UserAId, m.UserAFullName, m.LeagueIdentifier));
            }
            else
            {
                pendingRows.Add(BuildIdentityRow(
                    m.UserAId, m.UserAFullName, m.LeagueIdentifier));
            }

            // UserB only present for non-walkover matches.
            if (m.UserBId != null)
            {
                var userBId = m.UserBId.Value;
                var userBName = m.UserBFullName ?? string.Empty;
                if (bothSidesSubmitted)
                {
                    finishedRows.Add(BuildFinishedRow(
                        userBId, userBName, m.LeagueIdentifier,
                        m.UserBSolves, m.UserBBest, m.UserBAverage));
                }
                else if (m.UserBSubmitted)
                {
                    submittedAloneRows.Add(BuildIdentityRow(
                        userBId, userBName, m.LeagueIdentifier));
                }
                else
                {
                    pendingRows.Add(BuildIdentityRow(
                        userBId, userBName, m.LeagueIdentifier));
                }
            }
        }

        return new ActiveRoundLiveDetailDto
        {
            StartDate = round.StartDate,
            EndDate = round.EndDate,
            FinishedRows = finishedRows
                .OrderBy(r => r.Best.IsValid ? r.Best.Centiseconds : int.MaxValue)
                .ThenBy(r => r.LeagueIdentifier)
                .ThenBy(r => r.UserFullName)
                .ToList(),
            SubmittedAloneRows = submittedAloneRows
                .OrderBy(r => r.LeagueIdentifier)
                .ThenBy(r => r.UserFullName)
                .ToList(),
            PendingRows = pendingRows
                .OrderBy(r => r.LeagueIdentifier)
                .ThenBy(r => r.UserFullName)
                .ToList(),
        };
    }

    private static LiveRoundRowDto BuildFinishedRow(
        Guid userId, string fullName, string leagueIdentifier,
        List<SolveResult> solves, SolveResult best, SolveResult average)
        => new LiveRoundRowDto
        {
            UserId = userId,
            UserFullName = fullName,
            LeagueIdentifier = leagueIdentifier,
            Solve1 = solves[0],
            Solve2 = solves[1],
            Solve3 = solves[2],
            Solve4 = solves[3],
            Solve5 = solves[4],
            Best = best,
            Average = average,
        };

    private static LiveRoundRowDto BuildIdentityRow(
        Guid userId, string fullName, string leagueIdentifier)
        => new LiveRoundRowDto
        {
            UserId = userId,
            UserFullName = fullName,
            LeagueIdentifier = leagueIdentifier,
        };

    internal sealed class LiveRoundMatchProjection
    {
        public string LeagueIdentifier { get; set; } = string.Empty;
        public Guid UserAId { get; set; }
        public string UserAFullName { get; set; } = string.Empty;
        public bool UserASubmitted { get; set; }
        public SolveResult UserABest { get; set; }
        public SolveResult UserAAverage { get; set; }
        public List<SolveResult> UserASolves { get; set; } = new();
        public Guid? UserBId { get; set; }
        public string? UserBFullName { get; set; }
        public bool UserBSubmitted { get; set; }
        public SolveResult UserBBest { get; set; }
        public SolveResult UserBAverage { get; set; }
        public List<SolveResult> UserBSolves { get; set; } = new();
    }
}
