using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.LeagueSeasons.GetDetail;
using MediatR;

namespace BldLeague.Application.Queries.LeagueSeasons.GetDetail;

/// <summary>
/// Handles retrieving the detailed standings for a league season by season and league IDs, returning null if not found.
/// </summary>
public class GetLeagueSeasonDetailRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLeagueSeasonDetailRequest, LeagueSeasonDetailDto?>
{
    public async Task<LeagueSeasonDetailDto?> Handle(GetLeagueSeasonDetailRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.LeagueSeasonRepository.GetLeagueSeasonDetailAsync(request.SeasonId, request.LeagueId);
    }
}
