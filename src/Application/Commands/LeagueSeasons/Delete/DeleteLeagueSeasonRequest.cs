using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasons.Delete;

/// <summary>
/// Request to delete a league season by its unique identifier.
/// </summary>
public class DeleteLeagueSeasonRequest(Guid id) : IRequest<CommandResult>
{
    public Guid Id { get; set; } = id;
}
