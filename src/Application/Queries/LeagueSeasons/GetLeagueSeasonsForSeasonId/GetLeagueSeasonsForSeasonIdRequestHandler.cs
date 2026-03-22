using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.LeagueSeasons.GetLeagueSeasonsForSeasonId;

/// <summary>
/// Handles retrieving all league seasons for a given season ID as summary DTOs.
/// </summary>
public class GetLeagueSeasonsForSeasonIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLeagueSeasonsForSeasonIdRequest, IReadOnlyCollection<LeagueSeasonDto>>
{
    public async Task<IReadOnlyCollection<LeagueSeasonDto>> Handle(
        GetLeagueSeasonsForSeasonIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.LeagueSeasonRepository.GetLeagueSeasonsForSeasonIdAsync(request.SeasonId);
    }
}
