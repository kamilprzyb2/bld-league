using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using MediatR;

namespace BldLeague.Application.Commands.Seasons.Delete;

/// <summary>
/// Handles deleting a season, returning a failure result if the season does not exist.
/// </summary>
public class DeleteSeasonRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSeasonRequest, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteSeasonRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.SeasonRepository;
        var season = await repository.GetByIdAsync(request.Id);

        if (season == null)
            return CommandResult.FailGeneral($"Nie znaleziono sezonu z ID: {request.Id}.");

        repository.Delete(season);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok($"Usunięto sezon {season.SeasonName}.");
    }
}
