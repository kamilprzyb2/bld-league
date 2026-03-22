using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.LeagueSeasons.GetLeagueSeasonsForSeasonId;

/// <summary>
/// Request to retrieve all league seasons belonging to a specific season.
/// </summary>
public class GetLeagueSeasonsForSeasonIdRequest(Guid seasonId) : IRequest<IReadOnlyCollection<LeagueSeasonDto>>
{
    public Guid SeasonId { get; set; } = seasonId;
}
