using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Rounds.GetActiveFormUrl;
using BldLeague.Application.Queries.Rounds.GetAll;
using BldLeague.Application.Queries.Rounds.GetAllBySeasonId;
using BldLeague.Application.Queries.Rounds.GetDetail;
using BldLeague.Application.Queries.Rounds.GetScrambles;
using BldLeague.Domain.Entities;
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
            .Select(r => new RoundSummaryDto(r.Id, r.SeasonId, r.RoundNumber, r.StartDate, r.EndDate, r.SubmissionFormUrl))
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
            .Select(r => new RoundSummaryDto(r.Id, r.SeasonId, r.RoundNumber, r.StartDate, r.EndDate, r.SubmissionFormUrl))
            .FirstOrDefaultAsync();

    public async Task<int?> GetLatestRoundNumberAsync()
        => await DbSet
            .Select(r => (int?)r.RoundNumber)
            .MaxAsync();

    public async Task<ActiveRoundFormDto?> GetActiveRoundFormUrlAsync(DateTime utcNow)
    {
        var todayStart = utcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);
        return await DbSet
            .Where(r => r.StartDate < tomorrowStart && r.EndDate >= todayStart
                        && r.SubmissionFormUrl != null && r.SubmissionFormUrl != "")
            .Select(r => new ActiveRoundFormDto(r.SubmissionFormUrl!, "Kolejka " + r.RoundNumber))
            .FirstOrDefaultAsync();
    }

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
}
