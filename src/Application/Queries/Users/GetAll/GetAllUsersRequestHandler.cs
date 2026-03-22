using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Users.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetAll;

/// <summary>
/// Handles returning all users projected into summary DTOs.
/// </summary>
public class GetAllUsersRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllUsersRequest, IReadOnlyCollection<UserSummaryDto>>
{
    public async Task<IReadOnlyCollection<UserSummaryDto>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        return await unitOfWork.UserRepository.GetAllSummariesAsync();
    }
}
