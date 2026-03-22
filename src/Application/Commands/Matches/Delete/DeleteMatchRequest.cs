using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Delete;

/// <summary>
/// Request to delete a match by its unique identifier.
/// </summary>
public class DeleteMatchRequest(Guid id) : IRequest<CommandResult>
{
    public Guid Id { get; set; } = id;
}
