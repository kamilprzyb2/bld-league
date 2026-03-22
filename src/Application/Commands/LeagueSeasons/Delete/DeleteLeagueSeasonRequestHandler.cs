using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.LeagueSeasons.Delete;

/// <summary>
/// Handles deleting a league season, returning a failure result if the league season does not exist.
/// </summary>
public class DeleteLeagueSeasonRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteLeagueSeasonRequest, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteLeagueSeasonRequest request, CancellationToken cancellationToken)
    {
        var leagueSeason = await unitOfWork.LeagueSeasonRepository.GetByIdAsync(request.Id);
        if (leagueSeason == null)
            return CommandResult.FailGeneral($"Nie znaleziono liga-sezonu z ID: {request.Id}.");

        unitOfWork.LeagueSeasonRepository.Delete(leagueSeason);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok("Usunięto liga-sezon.");
    }
}
