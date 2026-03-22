using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Common;
using BldLeague.Domain.Entities;
using MediatR;

namespace BldLeague.Application.Commands.Seasons.Edit;

/// <summary>
/// Handles updating a season's number, rejecting the request if the season is not found or the new number conflicts.
/// </summary>
public class EditSeasonRequestHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<EditSeasonRequest, CommandResult>
{
    public async Task<CommandResult> Handle(EditSeasonRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.SeasonRepository;

        if (!await repository.ExistsAsync(s => s.Id == request.SeasonId))
            return CommandResult.FailGeneral($"Nie znaleziono sezonu z ID: {request.SeasonId}.");

        if (await repository.ExistsAsync(s => s.SeasonNumber == request.SeasonNumber && s.Id != request.SeasonId))
            return CommandResult.Fail(nameof(request.SeasonNumber), "Inny sezon z tym numerem już istnieje.");

        var season = new Season { Id = request.SeasonId, SeasonNumber = request.SeasonNumber };
        repository.Update(season);
        await unitOfWork.SaveAsync();
        return CommandResult.Ok($"Sezon {request.SeasonNumber} został zaktualizowany.");
    }
}
