using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasonUsers.SetGroup;

/// <summary>
/// Request to update the subleague group for multiple users within a league season in one operation.
/// </summary>
public class SetLeagueSeasonUserGroupsBatchRequest : IRequest<CommandResult>
{
    public Guid LeagueSeasonId { get; set; }
    public List<UserGroupEntry> Entries { get; set; } = [];

    public class UserGroupEntry
    {
        public Guid UserId { get; set; }
        public int SubleagueGroup { get; set; }
    }
}
