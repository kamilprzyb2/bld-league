using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.LeagueSeasons.GetAll;

/// <summary>
/// Request to retrieve all league seasons as a flat collection of summary DTOs.
/// </summary>
public class GetAllLeagueSeasonsRequest : IRequest<IReadOnlyCollection<LeagueSeasonDto>>;
