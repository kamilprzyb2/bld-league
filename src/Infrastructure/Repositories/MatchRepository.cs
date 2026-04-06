using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Matches.GetAll;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Application.Queries.Matches.GetMatchExport;
using BldLeague.Application.Queries.Matches.GetMatchSummaries;
using BldLeague.Application.Queries.Rounds.GetScrambles;
using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class MatchRepository(AppDbContext context)
    : ReadWriteRepositoryBase<Match>(context), IMatchRepository
{
    public async Task<IReadOnlyCollection<MatchAdminSummaryDto>> GetAllAdminSummariesAsync()
        => await DbSet
            .OrderByDescending(m => m.Round.RoundNumber)
            .ThenBy(m => m.LeagueSeason.League.LeagueIdentifier)
            .Select(m => new MatchAdminSummaryDto(
                m.Id,
                m.LeagueSeason.Season.SeasonNumber,
                m.LeagueSeason.League.LeagueIdentifier,
                m.Round.RoundNumber,
                m.UserA.FullName,
                m.UserB != null ? m.UserB.FullName : null,
                m.UserAScore,
                m.UserBScore))
            .ToListAsync();

    public async Task<IReadOnlyCollection<MatchExportRowDto>> GetMatchExportRowsAsync(int? seasonNumber = null, string? leagueIdentifier = null, int? roundNumber = null)
    {
        var query = DbSet.AsQueryable();
        if (seasonNumber.HasValue)
            query = query.Where(m => m.LeagueSeason.Season.SeasonNumber == seasonNumber.Value);
        if (leagueIdentifier != null)
            query = query.Where(m => m.LeagueSeason.League.LeagueIdentifier == leagueIdentifier);
        if (roundNumber.HasValue)
            query = query.Where(m => m.Round.RoundNumber == roundNumber.Value);
        return await query
            .OrderByDescending(m => m.LeagueSeason.Season.SeasonNumber)
            .ThenBy(m => m.LeagueSeason.League.LeagueIdentifier)
            .ThenBy(m => m.Round.RoundNumber)
            .Select(m => new MatchExportRowDto
            {
                SeasonNumber = m.LeagueSeason.Season.SeasonNumber,
                LeagueIdentifier = m.LeagueSeason.League.LeagueIdentifier,
                RoundNumber = m.Round.RoundNumber,
                UserAWcaId = m.UserA.WcaId,
                UserBWcaId = m.UserB != null ? m.UserB.WcaId : null,
                SolvesUserA = m.Solves
                    .Where(s => s.UserId == m.UserAId)
                    .OrderBy(s => s.Index)
                    .Select(s => s.Result)
                    .ToList(),
                SolvesUserB = m.Solves
                    .Where(s => s.UserId == m.UserBId)
                    .OrderBy(s => s.Index)
                    .Select(s => s.Result)
                    .ToList(),
            })
            .ToListAsync();
    }

    public async Task<MatchDetailsDto?> GetMatchDetailsByIdAsync(Guid id)
        => await DbSet
            .Where(m => m.Id == id)
            .Select(m => new MatchDetailsDto
            {
                Id = m.Id,
                SeasonId = m.LeagueSeason.SeasonId,
                LeagueId = m.LeagueSeason.LeagueId,
                RoundNumber = m.Round.RoundNumber,
                SeasonName = m.LeagueSeason.Season.SeasonName,
                LeagueName = m.LeagueSeason.League.LeagueName,
                RoundName = m.Round.RoundName,
                UserAFullName = m.UserA.FullName,
                UserBFullName = m.UserB != null ? m.UserB.FullName : null,
                SolvesUserA = m.Solves
                    .Where(s => s.UserId == m.UserAId)
                    .OrderBy(s => s.Index)
                    .Select(s => s.Result)
                    .ToList(),
                SolvesUserB = m.Solves
                    .Where(s => s.UserId == m.UserBId)
                    .OrderBy(s => s.Index)
                    .Select(s => s.Result)
                    .ToList(),
                UserAScore = m.UserAScore,
                UserBScore = m.UserBScore,
                RoundStartDate = m.Round.StartDate,
                RoundEndDate = m.Round.EndDate,
                UserABest = m.UserABest,
                UserBBest = m.UserBBest,
                UserAAverage = m.UserAAverage,
                UserBAverage = m.UserBAverage,
                Scrambles = m.Round.Scrambles
                    .OrderBy(s => s.ScrambleNumber)
                    .Select(s => new ScrambleDto { ScrambleNumber = s.ScrambleNumber, Notation = s.Notation })
                    .ToList(),
            })
            .FirstOrDefaultAsync();

    public async Task<IReadOnlyCollection<MatchSummaryDto>> GetMatchSummaries(Guid? seasonId, Guid? leagueId, int? roundNumber)
    {
        var query = DbSet.AsQueryable();

        if (seasonId.HasValue)
            query = query.Where(m => m.LeagueSeason.SeasonId == seasonId.Value);

        if (leagueId.HasValue)
            query = query.Where(m => m.LeagueSeason.LeagueId == leagueId.Value);

        if (roundNumber.HasValue)
            query = query.Where(m => m.Round.RoundNumber == roundNumber.Value);

        return await query
            .OrderByDescending(m => m.Round.RoundNumber)
            .ThenBy(m => m.LeagueSeason.League.LeagueIdentifier)
            .Select(m => new MatchSummaryDto
            {
                Id = m.Id,
                UserAFullName = m.UserA.FullName,
                UserBFullName = m.UserB != null ? m.UserB.FullName : null,
                UserAScore = m.UserAScore,
                UserBScore = m.UserBScore,
                RoundStartDate = m.Round.StartDate,
                RoundEndDate = m.Round.EndDate
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<Match>> GetMatchesByRoundIdAsync(Guid roundId)
        => await DbSet
            .Include(m => m.LeagueSeason)
            .Include(m => m.Solves)
            .Where(m => m.RoundId == roundId)
            .ToListAsync();

    public async Task<IReadOnlyCollection<Match>> GetMatchesByLeagueSeasonAsync(Guid leagueSeasonId)
        => await DbSet
            .Where(m => m.LeagueSeasonId == leagueSeasonId)
            .ToListAsync();

    public async Task<IReadOnlyCollection<Match>> GetFinishedMatchesByLeagueSeasonAsync(Guid leagueSeasonId, DateTime cutoff)
        => await DbSet
            .Where(m => m.LeagueSeasonId == leagueSeasonId && m.Round.EndDate < cutoff)
            .ToListAsync();

    public async Task<Match?> GetMatchWithSolvesAsync(Guid id)
        => await DbSet
            .Include(m => m.Solves)
            .FirstOrDefaultAsync(m => m.Id == id);
}
