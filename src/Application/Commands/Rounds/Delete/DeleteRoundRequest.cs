using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Rounds.Delete;

/// <summary>
/// Request to delete a round by its unique identifier.
/// </summary>
public class DeleteRoundRequest(Guid id) : IRequest<CommandResult>
{
    public Guid Id { get; set; } = id;
}
