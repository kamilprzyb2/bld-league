using BldLeague.Application.Common;
using BldLeague.Application.Validation;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Matches.Submit;

public record SubmitMatchSolvesRequest(
    Guid UserId,
    List<SubmitSolveDto> Solves
) : IRequest<CommandResult>;

public class SubmitSolveDto
{
    [SolveResult]
    public string? Result { get; init; } = string.Empty;
}
