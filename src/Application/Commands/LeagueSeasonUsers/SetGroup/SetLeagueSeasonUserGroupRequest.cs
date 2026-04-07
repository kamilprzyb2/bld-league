using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonUsers.SetGroup;

/// <summary>
/// Request to update the subleague group for a specific user within a league season.
/// </summary>
public class SetLeagueSeasonUserGroupRequest : IRequest<CommandResult>
{
    public Guid LeagueSeasonId { get; set; }
    public Guid UserId { get; set; }
    public int SubleagueGroup { get; set; }
}
