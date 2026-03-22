using BldLeague.Application.Common;
using BldLeague.Application.Validation;
using MediatR;

namespace BldLeague.Application.Commands.Leagues.Create;

/// <summary>
/// Request to create a new league with the specified identifier.
/// </summary>
public class CreateLeagueRequest : IRequest<CommandResult>
{
    [RequiredPl]
    public string LeagueIdentifier { get; set; } = string.Empty;
}
