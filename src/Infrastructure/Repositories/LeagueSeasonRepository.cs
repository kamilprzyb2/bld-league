using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using BldLeague.Application.Queries.LeagueSeasons.GetDetail;
using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class LeagueSeasonRepository(AppDbContext context) :
    ReadWriteRepositoryBase<LeagueSeason>(context), ILeagueSeasonRepository
{
    public async Task<IReadOnlyCollection<LeagueSeasonDto>> GetLeagueSeasonsForSeasonIdAsync(Guid seasonId)
        => await DbSet
            .Where(ls => ls.SeasonId == seasonId)
            .OrderBy(ls => ls.League.LeagueIdentifier)
            .Select(ls => new LeagueSeasonDto
            {
                LeagueSeasonId = ls.Id,
                LeagueId = ls.LeagueId,
                LeagueIdentifier = ls.League.LeagueIdentifier,
                LeagueName = ls.League.LeagueName,
                SeasonNumber = ls.Season.SeasonNumber,
                SeasonName =  ls.Season.SeasonName,
                SeasonId = ls.SeasonId,
                UserCount = ls.LeagueSeasonUsers.Count
            })
            .ToListAsync();

    public async Task<IReadOnlyCollection<LeagueSeasonDto>> GetAllProjectedAsync()
        => await DbSet
            .OrderByDescending(ls => ls.Season.SeasonNumber)
            .ThenBy(ls => ls.League.LeagueIdentifier)
            .Select(ls => new LeagueSeasonDto
            {
                LeagueSeasonId = ls.Id,
                LeagueId = ls.LeagueId,
                LeagueIdentifier = ls.League.LeagueIdentifier,
                LeagueName = ls.League.LeagueName,
                SeasonNumber = ls.Season.SeasonNumber,
                SeasonName = ls.Season.SeasonName,
                SeasonId = ls.SeasonId,
                UserCount = ls.LeagueSeasonUsers.Count
            })
            .ToListAsync();

    public async Task<LeagueSeasonDto?> GetProjectedByIdAsync(Guid id)
        => await DbSet
            .Where(ls => ls.Id == id)
            .Select(ls => new LeagueSeasonDto
            {
                LeagueSeasonId = ls.Id,
                LeagueId = ls.LeagueId,
                LeagueIdentifier = ls.League.LeagueIdentifier,
                LeagueName = ls.League.LeagueName,
                SeasonNumber = ls.Season.SeasonNumber,
                SeasonName = ls.Season.SeasonName,
                SeasonId = ls.SeasonId,
                UserCount = ls.LeagueSeasonUsers.Count
            })
            .FirstOrDefaultAsync();

    public async Task<LeagueSeasonDetailDto?> GetLeagueSeasonDetailAsync(Guid seasonId, Guid leagueId)
        => await DbSet
            .Where(ls => ls.SeasonId == seasonId && ls.LeagueId == leagueId)
            .Select(ls => new LeagueSeasonDetailDto
            {
                Standings = ls.LeagueSeasonStandings
                    .OrderBy(lss => lss.Place)
                    .ThenBy(lss => lss.User.FullName)
                    .Select(lss => new LeagueSeasonStandingDto
                    {
                        UserFullName = lss.User.FullName,
                        Place = lss.Place,
                        MatchesPlayed = lss.MatchesPlayed,
                        MatchesWon = lss.MatchesWon,
                        MatchesTied = lss.MatchesTied,
                        MatchesLost = lss.MatchesLost,
                        BigPoints = lss.BigPoints,
                        BonusPoints = lss.BonusPoints,
                        SmallPointsGained = lss.SmallPointsGained,
                        SmallPointsLost = lss.SmallPointsLost,
                        SmallPointsBalance = lss.SmallPointsBalance,
                        Best = lss.Best
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
}
