using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.LeagueSeasons.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.LeagueSeasons.GetById;

/// <summary>
/// Handles retrieving a league season summary DTO by ID, returning null if not found.
/// </summary>
public class GetLeagueSeasonByIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLeagueSeasonByIdRequest, LeagueSeasonDto?>
{
    public async Task<LeagueSeasonDto?> Handle(GetLeagueSeasonByIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.LeagueSeasonRepository.GetProjectedByIdAsync(request.LeagueSeasonId);
    }
}
