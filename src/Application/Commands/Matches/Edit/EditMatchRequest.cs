using System.ComponentModel.DataAnnotations;
using BldLeague.Application.Common;
using BldLeague.Application.Queries.Matches.GetMatchDetailsById;
using BldLeague.Application.Validation;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Edit;

/// <summary>
/// Request to update the solve results of an existing match identified by its ID.
/// </summary>
public class EditMatchRequest : IRequest<CommandResult>
{
    [Required][NotEmptyGuid]
    public Guid MatchId { get; set; }

    public List<SolveDto> UserASolves { get; set; } = new(Match.SOLVES_PER_MATCH);
    public List<SolveDto> UserBSolves { get; set; } = new(Match.SOLVES_PER_MATCH);
}
