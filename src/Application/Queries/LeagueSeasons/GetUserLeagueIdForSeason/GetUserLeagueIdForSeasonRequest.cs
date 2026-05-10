using MediatR;

namespace BldLeague.Application.Queries.LeagueSeasons.GetUserLeagueIdForSeason;

public record GetUserLeagueIdForSeasonRequest(Guid UserId, Guid SeasonId) : IRequest<Guid?>;
