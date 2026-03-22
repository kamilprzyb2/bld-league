using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Leagues.Delete;

/// <summary>
/// Request to delete a league by its unique identifier.
/// </summary>
public class DeleteLeagueRequest(Guid id) : IRequest<CommandResult>
{
    public Guid Id { get; set; } = id;
}
