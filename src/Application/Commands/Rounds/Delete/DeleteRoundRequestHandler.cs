using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Rounds.Delete;

/// <summary>
/// Handles deleting a round, returning a failure result if the round does not exist.
/// </summary>
public class DeleteRoundRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteRoundRequest, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteRoundRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.RoundRepository;
        var round = await repository.GetByIdAsync(request.Id);

        if (round == null)
        {
            return CommandResult.FailGeneral($"Nie znaleziono kolejki z ID: {request.Id}.");
        }

        repository.Delete(round);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Usunięto kolejkę.");
    }
}
