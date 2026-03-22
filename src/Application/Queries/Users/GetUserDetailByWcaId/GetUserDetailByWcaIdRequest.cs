using BldLeague.Application.Queries.Users.GetUserDetailByWcaId;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetUserDetailByWcaId;

/// <summary>
/// Request to retrieve full user details by WCA ID.
/// </summary>
public class GetUserDetailByWcaIdRequest(string wcaId) : IRequest<UserDetailDto?>
{
    public string WcaId { get; set; } = wcaId;
}
