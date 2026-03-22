using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Seasons.Edit;

/// <summary>
/// Request to update the season number of an existing season by its ID.
/// </summary>
public class EditSeasonRequest : IRequest<CommandResult>
{
    public Guid SeasonId { get; set; }
    public int SeasonNumber { get; set; }
}
