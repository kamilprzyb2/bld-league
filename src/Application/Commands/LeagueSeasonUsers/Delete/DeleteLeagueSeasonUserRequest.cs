using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonUsers.Delete;

/// <summary>
/// Request to remove a specific user from a league season by user ID and league season ID.
/// </summary>
public class DeleteLeagueSeasonUserRequest : IRequest<CommandResult>
{
    public Guid UserId { get; set; }
    public Guid LeagueSeasonId { get; set; }
}
