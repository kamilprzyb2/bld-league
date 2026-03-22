using BldLeague.Application.Queries.Users.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetById;

/// <summary>
/// Request to retrieve a single user summary DTO by their unique identifier.
/// </summary>
public class GetUserByIdRequest : IRequest<UserSummaryDto?>
{
    public Guid UserId { get; set; }
}
