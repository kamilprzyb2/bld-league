using BldLeague.Application.Queries.Users.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetAll;

/// <summary>
/// Request to retrieve all users as a collection of summary DTOs.
/// </summary>
public class GetAllUsersRequest : IRequest<IReadOnlyCollection<UserSummaryDto>>;
