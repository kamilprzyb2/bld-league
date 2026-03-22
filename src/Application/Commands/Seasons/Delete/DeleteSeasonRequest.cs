using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Seasons.Delete;

/// <summary>
/// Request to delete a season by its unique identifier.
/// </summary>
public class DeleteSeasonRequest(Guid id) : IRequest<CommandResult>
{
    public Guid Id { get; set; } = id;
}
