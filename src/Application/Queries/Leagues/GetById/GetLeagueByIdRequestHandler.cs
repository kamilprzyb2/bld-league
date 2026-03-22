using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Leagues.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Leagues.GetById;

/// <summary>
/// Handles retrieving a league summary DTO by ID, returning null if not found.
/// </summary>
public class GetLeagueByIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLeagueByIdRequest, LeagueSummaryDto?>
{
    public async Task<LeagueSummaryDto?> Handle(GetLeagueByIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.LeagueRepository.GetSummaryByIdAsync(request.LeagueId);
    }
}
