using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Users.GetUserDetailByWcaId;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetUserDetailByWcaId;

/// <summary>
/// Handles retrieving full user details by WCA ID, returning null if no matching user exists.
/// </summary>
public class GetUserDetailByWcaIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserDetailByWcaIdRequest, UserDetailDto?>
{
    public Task<UserDetailDto?> Handle(GetUserDetailByWcaIdRequest request, CancellationToken cancellationToken)
    {
        return unitOfWork.UserRepository.GetUserDetailByWcaIdAsync(request.WcaId);
    }
}
