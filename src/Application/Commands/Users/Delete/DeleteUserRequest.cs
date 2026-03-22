using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Users.Delete;

/// <summary>
/// Request to delete a user by their unique identifier.
/// </summary>
public class DeleteUserRequest(Guid id) : IRequest<CommandResult>
{
    public Guid Id { get; set; } = id;
}
