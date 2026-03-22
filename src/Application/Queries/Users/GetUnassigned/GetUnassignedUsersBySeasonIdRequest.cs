using BldLeague.Application.Queries.Users.GetAll;
using MediatR;

namespace BldLeague.Application.Queries.Users.GetUnassigned;

/// <summary>
/// Request to retrieve all users not yet assigned to any league within a specific season.
/// </summary>
public class GetUnassignedUsersBySeasonIdRequest(Guid seasonId) : IRequest<IReadOnlyCollection<UserSummaryDto>>
{
    public Guid SeasonId { get; set; } = seasonId;
}
