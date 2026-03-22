using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Leagues.Delete;

/// <summary>
/// Handles deleting a league, returning a failure result if the league does not exist.
/// </summary>
public class DeleteLeagueRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteLeagueRequest, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteLeagueRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.LeagueRepository;
        var league = await repository.GetByIdAsync(request.Id);

        if (league == null)
            return CommandResult.FailGeneral($"Nie znaleziono ligi z ID: {request.Id}.");

        repository.Delete(league);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Usunięto ligę.");
    }
}
