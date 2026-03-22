using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Seasons.Create;

/// <summary>
/// Request to create a new season with the specified season number.
/// </summary>
public class CreateSeasonRequest : IRequest<CommandResult>
{
    public int SeasonNumber { get; set; }
}
