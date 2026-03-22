using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.LeagueSeasons.GetAll;

/// <summary>
/// Handles returning all league seasons projected into summary DTOs.
/// </summary>
public class GetAllLeagueSeasonsRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllLeagueSeasonsRequest, IReadOnlyCollection<LeagueSeasonDto>>
{
    public async Task<IReadOnlyCollection<LeagueSeasonDto>> Handle(GetAllLeagueSeasonsRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.LeagueSeasonRepository.GetAllProjectedAsync();
    }
}
