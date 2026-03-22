using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Users.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetUnassigned;

/// <summary>
/// Handles retrieving users not assigned to any league season for the given season ID.
/// </summary>
public class GetUnassignedUsersBySeasonIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUnassignedUsersBySeasonIdRequest, IReadOnlyCollection<UserSummaryDto>>
{
    public async Task<IReadOnlyCollection<UserSummaryDto>> Handle(GetUnassignedUsersBySeasonIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.UserRepository.GetUnassignedUsersBySeasonIdAsync(request.SeasonId);
    }
}
