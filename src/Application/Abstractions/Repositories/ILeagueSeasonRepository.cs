using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using BldLeague.Application.Queries.LeagueSeasons.GetDetail;
using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface ILeagueSeasonRepository : IReadWriteRepository<LeagueSeason>
{
    /// <summary>
    /// Retrieves all league seasons for the given season.
    /// </summary>
    /// <param name="seasonId">Requested season ID.</param>
    /// <returns>Collection of league seasons.</returns>
    Task<IReadOnlyCollection<LeagueSeasonDto>> GetLeagueSeasonsForSeasonIdAsync(Guid seasonId);
    Task<IReadOnlyCollection<LeagueSeasonDto>> GetAllProjectedAsync();
    
    Task<LeagueSeasonDto?> GetProjectedByIdAsync(Guid id);
    
    Task<LeagueSeasonDetailDto?> GetLeagueSeasonDetailAsync(Guid seasonId, Guid leagueId);
}