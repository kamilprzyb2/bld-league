using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonUsers.Create;

/// <summary>
/// Request to add one or more users to a league season roster.
/// </summary>
public class CreateLeagueSeasonUsersRequest : IRequest<CommandResult>
{
    public required Guid LeagueSeasonId { get; set; }
    public required List<Guid> UserIds { get; set; }
}
