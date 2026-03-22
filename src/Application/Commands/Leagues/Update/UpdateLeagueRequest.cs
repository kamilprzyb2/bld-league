using BldLeague.Application.Common;
using BldLeague.Application.Validation;
using MediatR;

namespace BldLeague.Application.Commands.Leagues.Update;

/// <summary>
/// Request to update the identifier of an existing league by its ID.
/// </summary>
public class UpdateLeagueRequest : IRequest<CommandResult>
{
    [RequiredPl]
    public string LeagueIdentifier { get; set; } = string.Empty;
    public Guid LeagueId { get; set; }
}
