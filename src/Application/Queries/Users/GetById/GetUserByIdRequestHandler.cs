using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Users.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetById;

/// <summary>
/// Handles retrieving a user summary DTO by ID, returning null if not found.
/// </summary>
public class GetUserByIdRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserByIdRequest, UserSummaryDto?>
{
    public async Task<UserSummaryDto?> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.UserRepository.GetSummaryByIdAsync(request.UserId);
    }
}
